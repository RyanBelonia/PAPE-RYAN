using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace InteriorPlanner.Systems.Tools
{
    public class PaintTool : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask paintableLayers; 

        private Material selectedMaterial;

        public void SetActiveMaterial(Material newMaterial)
        {
            selectedMaterial = newMaterial;
            Debug.Log($" microler carregado com: {newMaterial.name}");
        }

        public void ClearTool()
        {
            selectedMaterial = null;
        }

        private void Update()
        {
            if (Mouse.current == null || mainCamera == null || selectedMaterial == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;
                ApplyPaint();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearTool();
            }
        }

        private void ApplyPaint()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, paintableLayers))
            {
                int hitLayer = hit.collider.gameObject.layer;
                int placeableLayerIndex = LayerMask.NameToLayer("Placeable");

                // SE bater na layer de objetos/mobília, valida se tem a etiqueta de pintura
                if (hitLayer == placeableLayerIndex)
                {
                    Paintable paintableComponent = hit.collider.GetComponentInParent<Paintable>();
                    
                    if (paintableComponent == null)
                    {
                        Debug.Log("Este objeto não pode ser pintado.");
                        return; // Bloqueia a pintura em sofás/móveis comuns
                    }
                }

                // Se for parede normal, chão normal, ou passou no teste da etiqueta, pinta!
                Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    rend.material = selectedMaterial;
                }
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}