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

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}