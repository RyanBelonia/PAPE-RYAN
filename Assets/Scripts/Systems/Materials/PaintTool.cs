using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using InteriorPlanner.Systems.Placement;
    
namespace InteriorPlanner.Systems.Tools
{
    /// <summary>
    /// Ferramenta interativa de "Balde de Tinta". Dispara Raios (Raycasts) a partir do rato 
    /// para pintar paredes, chão e divisórias que tenham permissão para receber texturas.
    /// </summary>
    public class PaintTool : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        
        // Define quais as categorias físicas (Layers) que o raio de tinta consegue "ver".
        // Isto impede que a tinta atravesse as paredes ou pinte o céu.
        [SerializeField] private LayerMask paintableLayers;

        // Guarda em memória a textura que o utilizador escolheu no catálogo de materiais
        private Material selectedMaterial;

        /// <summary>
        /// Chamado pelos botões do menu de materiais para "molhar o pincel".
        /// </summary>
        public void SetActiveMaterial(Material newMaterial)
        {
            selectedMaterial = newMaterial;
            Debug.Log($"🖌️ Pincel carregado com: {newMaterial.name}");
        }

        /// <summary>
        /// "Lava o pincel" e impede pinturas acidentais.
        /// </summary>
        public void ClearTool()
        {
            selectedMaterial = null;
        }

        private void Update()
        {
            // Bloqueios de segurança: Não faz nada se faltar rato, câmara ou não houver tinta no pincel
            if (Mouse.current == null || mainCamera == null || selectedMaterial == null) return;

            // Clique Esquerdo: Tenta aplicar a tinta no objeto clicado
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Proteção UX: Não pinta a parede se o utilizador estiver a tentar clicar num botão da Interface
                if (IsPointerOverUI()) return;
                ApplyPaint();
            }

            // Clique Direito: Cancela a ferramenta de pintura atual
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearTool();
            }
        }

        /// <summary>
        /// Motor central de pintura (Física de Raios)
        /// </summary>
        private void ApplyPaint()
        {
            // Converte a coordenada 2D do rato no ecrã num raio 3D que viaja pela sala
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Ferramenta de depuração para o programador visualizar o percurso da tinta no editor
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            // Se o raio bater em alguma coisa que pertença às layers "Paintable" (Paredes, Chão, etc.) a até 500 metros de distância...
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, paintableLayers))
            {
                Debug.Log($"🎯 1. O raio bateu em: {hit.collider.gameObject.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                int hitLayer = hit.collider.gameObject.layer;
                int placeableLayerIndex = LayerMask.NameToLayer("Placeable");

                // SE FOR UM MÓVEL: Aplica lógica de filtragem para não pintar sofás e camas acidentalmente
                if (hitLayer == placeableLayerIndex)
                {
                    // Procura o "Passaporte" de pintura. Se não o tiver, a pintura é bloqueada.
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

                // --- APLICAÇÃO DA TINTA NA MEMÓRIA E NO GRÁFICO ---
                PlaceableObject placeable = hit.collider.GetComponentInParent<PlaceableObject>();

                // Se o objeto tiver inteligência espacial (como divisórias geradas pelo AutoConfigurer)
                if (placeable != null)
                {
                    // Não muda apenas a cor; atualiza a memória interna do objeto para que 
                    // o sistema de "Save" lembre que parede ficou com esta cor!
                    placeable.UpdateOriginalMaterial(selectedMaterial);
                    Debug.Log($"🎨 3. Sucesso! Divisória pintada e memória atualizada!");
                }
                else
                {
                    // Se for um chão ou parede gerada pelo RoomGenerator antigo (sem inteligência complexa)
                    Renderer rend = hit.collider.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = selectedMaterial;
                        Debug.Log($"🎨 3. Sucesso! Objeto simples pintado!");
                    }
                }
            }
        }
        
        /// <summary>
        /// Interroga a Interface Gráfica para saber se há botões debaixo do rato.
        /// </summary>
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}