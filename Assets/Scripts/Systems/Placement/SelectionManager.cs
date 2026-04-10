using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace InteriorPlanner.Systems.Placement
{
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask selectableLayerMask; // Mudar para "Furniture" no Inspector

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

    // Desenha uma linha vermelha na janela 'Scene' para veres o tiro do rato
    Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

    // Dispara contra TUDO primeiro para sabermos onde está a bater
    if (Physics.Raycast(ray, out RaycastHit hit, 500f))
    {
        Debug.Log($"[DEBUG] O Raio bateu em: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

        // Verifica se a layer do objeto que bateu faz parte da tua Layer Mask
        if (((1 << hit.collider.gameObject.layer) & selectableLayerMask) != 0)
        {
            PlaceableObject placeable = hit.collider.GetComponentInParent<PlaceableObject>();

            if (placeable != null)
            {
                Debug.Log("<color=green>[DEBUG] SUCESSO! Encontrou o script PlaceableObject!</color>");
                SelectObject(placeable);
                return;
            }
            else
            {
                Debug.LogWarning("<color=orange>[DEBUG] Bateu na Layer certa, mas NÃO achou o script PlaceableObject no objeto nem nos pais!</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=orange>[DEBUG] Bateu, mas a Layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}' NÃO está selecionada na tua Selectable Layer Mask!</color>");
        }
    }
    else
    {
        Debug.LogError("<color=red>[DEBUG] O Raio não bateu em NADA (nem móvel, nem chão, nem parede).</color>");
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

            Debug.Log("Selecionado: " + currentSelectedObject.name + " | Tipo: " + currentSelectedObject.ObjectType);
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}