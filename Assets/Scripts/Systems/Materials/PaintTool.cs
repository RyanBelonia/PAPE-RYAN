using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using InteriorPlanner.Systems.Placement;
    
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
            Debug.Log($"🖌️ Pincel carregado com: {newMaterial.name}");
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

            // Desenha um raio vermelho na cena (só se vê na janela 'Scene' enquanto jogas)
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, paintableLayers))
            {
                Debug.Log($"🎯 1. O raio bateu em: {hit.collider.gameObject.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                int hitLayer = hit.collider.gameObject.layer;
                int placeableLayerIndex = LayerMask.NameToLayer("Placeable");

                if (hitLayer == placeableLayerIndex)
                {
                    Paintable paintableComponent = hit.collider.GetComponentInParent<Paintable>();

                    if (paintableComponent == null)
                    {
                        Debug.Log("❌ 2. Bateu num Placeable, mas NÃO tem a etiqueta Paintable. Pintura cancelada.");
                        return;
                    }
                    else
                    {
                        Debug.Log("✅ 2. Encontrou a etiqueta Paintable no objeto!");
                    }
                }

                // --- CÓDIGO NOVO ---
                PlaceableObject placeable = hit.collider.GetComponentInParent<PlaceableObject>();

                if (placeable != null)
                {
                    // Se for um móvel/divisória, avisamos o script para ele atualizar a sua memória interna
                    placeable.UpdateOriginalMaterial(selectedMaterial);
                    Debug.Log($"🎨 3. Sucesso! Divisória pintada e memória atualizada!");
                }
                else
                {
                    // Se for um chão normal (sem script PlaceableObject), pintamos da forma normal
                    Renderer rend = hit.collider.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = selectedMaterial;
                        Debug.Log($"🎨 3. Sucesso! Objeto simples pintado!");
                    }
                }
            }
        }
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}