using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Placement
{
    /// <summary>
    /// O motor de física e locomoção do projeto. Controla a forma como o utilizador arrasta,
    /// roda e posiciona os móveis em tempo real. Implementa duas lógicas físicas distintas:
    /// - Navegação 2D no plano do chão (para móveis e divisórias).
    /// - "Snap" vertical para modelos estruturais (colar portas/janelas às paredes).
    /// </summary>
    public class ObjectMover : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        
        // Máscaras de colisão: Dizem aos raios físicos o que eles podem "tocar"
        [SerializeField] private LayerMask floorLayerMask; 
        [SerializeField] private LayerMask obstacleLayerMask; 
        [SerializeField] private LayerMask wallLayerMask;
        
        [SerializeField] private SelectionManager selectionManager;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeedScroll = 15f; // Rodar suavemente com a roda do rato
        [SerializeField] private float rotationSpeedKey = 45f;    // Rodar em ângulos precisos usando a tecla "R"

        [Header("Placement Settings")]
        [SerializeField] private bool useGridSnap = false;        // Modo CAD: move em saltos matemáticos exatos
        [SerializeField] private float gridSize = 0.5f; 
        [SerializeField] private bool useSmoothMovement = true;   // Modo Orgânico: o objeto "flutua" para seguir o rato
        [SerializeField] private float smoothSpeed = 15f;

        // Estado da locomoção
        private PlaceableObject currentObject;
        private bool isDragging;
        
        // O dragOffset garante que se tu clicares na ponta do sofá, 
        // ele é arrastado por essa ponta e não salta subitamente para o meio do rato.
        private Vector3 dragOffset; 

        private void LateUpdate()
        {
            // Proteção contra falhas de periféricos (previne NullReferenceException)
            if (Mouse.current == null || Keyboard.current == null || mainCamera == null || selectionManager == null)
                return;

            // FASE 1: INÍCIO DO ARRASTO (On Mouse Down)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Ignora o clique na sala se o utilizador estiver a carregar num botão do Menu UI
                if (IsPointerOverUI()) return;

                // Pergunta ao SelectionManager qual é o móvel ativo neste momento
                currentObject = selectionManager.GetSelectedObject();

                // O currentObject.CanMove impede o utilizador de tentar arrastar coisas trancadas, como o chão da sala
                if (currentObject != null && currentObject.CanMove)
                {
                    // Dispara o raio virtual que converte o ecrã 2D do rato num percurso no mundo 3D
                    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    
                    // --- RAMIFICAÇÃO: COMPORTAMENTO FÍSICO DO OBJETO ---
                    // Se o objeto estiver marcado pelo AutoConfigurer como sendo de parede (Portas/Janelas)...
                    if (currentObject.RequiresWallSupport)
                    {
                        // O raio ignora o chão e procura apenas pelas máscaras "Wall"
                        if (Physics.Raycast(ray, out RaycastHit hit, 500f, wallLayerMask))
                        {
                            // Para objetos de parede, calcula a diferença entre o centro da janela e o sítio onde bateu
                            dragOffset = currentObject.transform.position - hit.point;
                            isDragging = true;
                        }
                    }
                    // Se o objeto for um móvel normal de chão...
                    else
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                        {
                            dragOffset = currentObject.transform.position - hit.point;
                            
                            // Anulamos o Y para que o rato não enterre o sofá no chão sem querer
                            dragOffset.y = 0; 
                            isDragging = true;
                        }
                    }
                }
            }

            // FASE 2: DURANTE O ARRASTO (On Mouse Drag)
            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                // Se o objeto for apagado enquanto está a ser movido, cancela a ação
                if (currentObject == null)
                {
                    isDragging = false;
                    return;
                }

                MoveObjectWithMouse();
                RotateObjectWithInput();
                
                // O CheckCollisions pinta o objeto de vermelho se ele estiver enfiado noutro móvel
                CheckCollisions(); 
            }

            // FASE 3: FIM DO ARRASTO (On Mouse Up)
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // Só larga o objeto se ele não estiver numa posição inválida (a vermelho)
                if (currentObject != null && currentObject.IsValidPosition)
                {
                    isDragging = false;
                    currentObject = null;
                }
            }
        }

        /// <summary>
        /// Translada o objeto 3D acompanhando as coordenadas do ecrã do rato.
        /// </summary>
        private void MoveObjectWithMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // --- RAMIFICAÇÃO 1: OBJETOS DE PAREDE (Arrastar pela parede) ---
            if (currentObject.RequiresWallSupport)
            {
                // Dispara o raio contra TODAS as paredes para permitir que uma janela "dê a volta" ao quarto e mude de parede
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, wallLayerMask))
                {
                    Vector3 targetPos = hit.point;
                    
                    // Nota: Para portas/janelas, pode-se manter a altura fixa (targetPos.y = ...) 
                    // ou deixar que deslizem livremente verticalmente (como está agora).

                    currentObject.transform.position = targetPos;
                    
                    // Matemátia Avançada: Quaternion.LookRotation(hit.normal) diz à janela 
                    // para se rodar automaticamente de forma a ficar de "costas" colada para a parede que o rato está a tocar!
                    currentObject.transform.rotation = Quaternion.LookRotation(hit.normal);
                }
                // Se o rato sair da parede e for para o meio da sala, a janela fica "paralisada" no último ponto válido.
            }
            
            // --- RAMIFICAÇÃO 2: OBJETOS DE CHÃO (Móveis / Divisórias) ---
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                {
                    // Aplica o offset para evitar "solavancos" da mobília ao arrastar
                    Vector3 targetPosition = hit.point + dragOffset;
                    
                    // Tranca a altura do móvel para ele não levantar voo
                    targetPosition.y = currentObject.transform.position.y;

                    // Aplica arredondamento matemático para que o móvel ande aos "saltinhos" (Snap to Grid) como num software CAD
                    if (useGridSnap)
                    {
                        targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                        targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
                    }

                    // Impede o móvel de ser arrastado para fora do chão gerado
                    targetPosition = ClampPositionToRoom(targetPosition);

                    // Movimento interpolado (Lerp) que dá um aspeto muito mais natural e pesado ao deslizar móveis
                    if (useSmoothMovement && !useGridSnap) 
                    {
                        currentObject.transform.position = Vector3.Lerp(currentObject.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
                    }
                    else
                    {
                        currentObject.transform.position = targetPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Usa matemática volumétrica para criar uma "Caixa Fantasma" em redor do móvel selecionado 
        /// e deteta se essa caixa colide fisicamente com outros móveis da sala.
        /// </summary>
        private void CheckCollisions()
        {
            BoxCollider boxCol = currentObject.GetComponentInChildren<BoxCollider>();
            if (boxCol == null) return;

            // Converte o centro do collider (espaço local) para espaço mundial (onde o móvel está na sala)
            Vector3 worldCenter = boxCol.transform.TransformPoint(boxCol.center);
            
            // Pega na escala e nos limites da caixa. 
            // O * 0.95f encolhe a caixa virtual em 5% para dar uma pequena "folga" e permitir que móveis fiquem mesmo encostados sem dar erro de colisão.
            Vector3 halfExtents = Vector3.Scale(boxCol.size, boxCol.transform.lossyScale) * 0.5f;
            halfExtents *= 0.95f; 

            // Devolve todos os móveis que a nossa "caixa fantasma" está a tocar neste momento
            Collider[] hits = Physics.OverlapBox(worldCenter, halfExtents, boxCol.transform.rotation, obstacleLayerMask);

            bool isColliding = false;
            foreach (Collider hit in hits)
            {
                // Se a caixa tocou nalgum Collider que não pertença a ela própria (ao próprio móvel)...
                if (hit.gameObject != currentObject.gameObject && hit.transform.root != currentObject.transform.root)
                {
                    isColliding = true;
                    break;
                }
            }

            // Avisa o objeto do resultado. Se for falso (isColliding = true), o material do móvel vai ficar a piscar a vermelho.
            currentObject.SetValidState(!isColliding);
        }

        /// <summary>
        /// Proteção arquitetónica: Não permite que o utilizador mova o sofá para fora da casa (atravessando as paredes).
        /// </summary>
        private Vector3 ClampPositionToRoom(Vector3 targetPos)
        {
            // Se as bases de dados da sala não existirem na RAM, desliga a proteção.
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
                return targetPos;

            RoomData room = AppManager.Instance.ProjectSession.CurrentProject.Room;
            if (room == null) return targetPos;

            // Descobre o tamanho real (físico) do objeto para que o limite considere a "ponta" do objeto e não o centro.
            // Ex: Um sofá de 2 metros não pode ir até à beira da parede a contar do centro, senão 1 metro do sofá entra pela parede a dentro.
            Collider col = currentObject.GetComponentInChildren<Collider>();
            float extentsX = col != null ? col.bounds.extents.x : 0f;
            float extentsZ = col != null ? col.bounds.extents.z : 0f;

            // Calcula a fronteira da sala (Metade do tamanho para a esquerda, metade para a direita)
            float minX = (-room.Width / 2f) + extentsX;
            float maxX = (room.Width / 2f) - extentsX;
            
            float minZ = (-room.Length / 2f) + extentsZ;
            float maxZ = (room.Length / 2f) - extentsZ;

            // Mathf.Clamp tranca os valores e impede que subam acima do máximo ou desçam abaixo do mínimo
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

            return targetPos;
        }

        /// <summary>
        /// Ouve a tecla "R" ou o deslize do Rato para girar o objeto no eixo Y.
        /// </summary>
        private void RotateObjectWithInput()
        {
            if (!currentObject.CanRotate) return; 

            // Rotação abrupta (ex: 45 graus)
            if (Keyboard.current.rKey.wasPressedThisFrame)
                currentObject.transform.Rotate(Vector3.up, rotationSpeedKey);

            // Rotação micrométrica (rodinha do rato)
            float scrollValue = Mouse.current.scroll.y.ReadValue();
            if (scrollValue > 0)
                currentObject.transform.Rotate(Vector3.up, rotationSpeedScroll);
            else if (scrollValue < 0)
                currentObject.transform.Rotate(Vector3.up, -rotationSpeedScroll);
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}