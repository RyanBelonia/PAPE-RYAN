using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace InteriorPlanner.Systems.Placement
{
    /// <summary>
    /// O Sistema Nervoso de Interação. Acompanha os cliques do rato na cena 3D, 
    /// identifica em que objeto o utilizador tocou, e gere quem é o único objeto 
    /// selecionado (ativo) no momento.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        
        // Define as Layers (Camadas Físicas) onde o rato tem permissão para clicar (ex: Furniture)
        [SerializeField] private LayerMask selectableLayerMask; 
        
        // Máscara dedicada para a inteligência artificial de colagem de janelas
        [SerializeField] private LayerMask wallLayerMask; 

        // Guarda o ponteiro de memória para o móvel que está atualmente a amarelo
        private PlaceableObject currentSelectedObject;

        private void Update()
        {
            // Segurança contra componentes nulls
            if (Mouse.current == null || mainCamera == null)
                return;

            // Se o botão esquerdo foi clicado neste frame...
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Aborta a tentativa de seleção no espaço 3D se o rato estiver em cima de um painel da UI 2D
                if (IsPointerOverUI())
                    return;

                TrySelectObject();
            }
        }

        public PlaceableObject GetSelectedObject()
        {
            return currentSelectedObject;
        }

        /// <summary>
        /// Retira o foco do objeto atual e avisa-o para desligar o amarelo.
        /// </summary>
        public void ClearSelection()
        {
            if (currentSelectedObject != null)
            {
                currentSelectedObject.Deselect();
                currentSelectedObject = null;
            }
        }

        /// <summary>
        /// Dispara o Raycast (física ótica) a partir do rato para tentar "apanhar" um móvel.
        /// </summary>
        private void TrySelectObject()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                // Matemática de Bits (Bitwise): Compara o código binário da Layer do objeto 
                // com a LayerMask configurada no Inspector para ver se existe correspondência.
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

            // Se o raio bateu no vazio ou no chão (fora da LayerMask), limpa a seleção
            ClearSelection();
        }

        private void SelectObject(PlaceableObject newSelection)
        {
            // Evita processamento desnecessário se clicarmos no mesmo objeto duas vezes
            if (currentSelectedObject == newSelection)
                return;

            if (currentSelectedObject != null)
            {
                currentSelectedObject.Deselect();
            }

            currentSelectedObject = newSelection;
            currentSelectedObject.Select();
        }

        // =========================================================================
        // LÓGICA DE INSTANCIAÇÃO INTELIGENTE (SMART SPAWN)
        // =========================================================================
        
        /// <summary>
        /// Força o sistema a selecionar um objeto recém-criado.
        /// Se esse objeto for do tipo estrutural (porta/janela), ele corre um algoritmo
        /// de busca espacial para o colar à parede mais próxima, evitando que fique a flutuar na sala.
        /// </summary>
        public void ForceSelectAndSnap(PlaceableObject newObject)
        {
            if (newObject == null) return;

            // Ativa o sistema "Magnético" se o objeto não suportar chão
            if (newObject.RequiresWallSupport)
            {
                SnapToNearestWall(newObject.transform);
            }

            // Força a seleção sem exigir que o utilizador clique nele fisicamente
            SelectObject(newObject);
        }

        /// <summary>
        /// Algoritmo de busca espacial em duas fases para encontrar um hospedeiro (parede).
        /// </summary>
        private void SnapToNearestWall(Transform objTransform)
        {
            // 1ª TENTATIVA: O utilizador está a olhar diretamente para uma parede? 
            // Dispara um raio a partir do centro do ecrã do PC.
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, wallLayerMask))
            {
                // Se sim, teleporta o objeto para o local exato onde o utilizador estava a olhar
                objTransform.position = hit.point;
                // E alinha-o com as normais da face da parede para que não fique torto
                objTransform.rotation = Quaternion.LookRotation(hit.normal);
                return; // Missão cumprida
            }

            // 2ª TENTATIVA: O utilizador estava a olhar para o teto ou para o chão.
            // O sistema ativa o "Modo Radar" - Dispara 4 raios nas quatro direções cardeais a partir da câmara.
            Vector3 origin = new Vector3(mainCamera.transform.position.x, 1.5f, mainCamera.transform.position.z);
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
            float closestDistance = Mathf.Infinity;
            Vector3 bestPos = Vector3.zero;
            Vector3 bestNormal = Vector3.forward;

            // Varre as 4 direções e mede as distâncias
            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(origin, dir, out RaycastHit searchHit, 100f, wallLayerMask))
                {
                    // Encontra e guarda a parede com a distância mais curta
                    if (searchHit.distance < closestDistance)
                    {
                        closestDistance = searchHit.distance;
                        bestPos = searchHit.point;
                        bestNormal = searchHit.normal;
                    }
                }
            }

            // Teleporta o objeto para a parede vencedora do teste "Modo Radar"
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