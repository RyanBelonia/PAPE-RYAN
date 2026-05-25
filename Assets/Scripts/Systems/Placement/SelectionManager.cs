using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace InteriorPlanner.Systems.Placement
{
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask selectableLayerMask; // Mudar para "Furniture" no Inspector
        [SerializeField] private LayerMask wallLayerMask; // NOVA: Máscara para detetar Paredes

        private PlaceableObject currentSelectedObject;

        private void Update()
        {
            if (Mouse.current == null || mainCamera == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI())
                    return;

                TrySelectObject();
            }
        }

        public PlaceableObject GetSelectedObject()
        {
            return currentSelectedObject;
        }

        public void ClearSelection()
        {
            if (currentSelectedObject != null)
            {
                currentSelectedObject.Deselect();
                currentSelectedObject = null;
            }
        }

        private void TrySelectObject()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                // Verifica se a layer do objeto que bateu faz parte da tua Layer Mask
                if (((1 << hit.collider.gameObject.layer) & selectableLayerMask) != 0)
                {
                    PlaceableObject placeable = hit.collider.GetComponentInParent<PlaceableObject>();

                    if (placeable != null)
                    {
                        SelectObject(placeable);
                        return;
                    }
                }
            }

            ClearSelection();
        }

        private void SelectObject(PlaceableObject newSelection)
        {
            if (currentSelectedObject == newSelection)
                return;

            if (currentSelectedObject != null)
            {
                currentSelectedObject.Deselect();
            }

            currentSelectedObject = newSelection;
            currentSelectedObject.Select();
        }

        // --- NOVA LÓGICA: FORÇAR SELEÇÃO E COLAR NA PAREDE ---
        
        public void ForceSelectAndSnap(PlaceableObject newObject)
        {
            if (newObject == null) return;

            // Se for uma Porta/Janela, cola na parede antes de ser selecionado
            if (newObject.RequiresWallSupport)
            {
                SnapToNearestWall(newObject.transform);
            }

            // Força a seleção do novo objeto como se tivesses clicado nele
            SelectObject(newObject);
        }

        private void SnapToNearestWall(Transform objTransform)
        {
            // 1. Tenta disparar para o centro do ecrã
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, wallLayerMask))
            {
                objTransform.position = hit.point;
                objTransform.rotation = Quaternion.LookRotation(hit.normal);
                return;
            }

            // 2. Se o centro do ecrã não bater numa parede, procura a parede mais próxima num raio em cruz
            Vector3 origin = new Vector3(mainCamera.transform.position.x, 1.5f, mainCamera.transform.position.z);
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
            float closestDistance = Mathf.Infinity;
            Vector3 bestPos = Vector3.zero;
            Vector3 bestNormal = Vector3.forward;

            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(origin, dir, out RaycastHit searchHit, 100f, wallLayerMask))
                {
                    if (searchHit.distance < closestDistance)
                    {
                        closestDistance = searchHit.distance;
                        bestPos = searchHit.point;
                        bestNormal = searchHit.normal;
                    }
                }
            }

            if (closestDistance < Mathf.Infinity)
            {
                objTransform.position = bestPos;
                objTransform.rotation = Quaternion.LookRotation(bestNormal);
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}