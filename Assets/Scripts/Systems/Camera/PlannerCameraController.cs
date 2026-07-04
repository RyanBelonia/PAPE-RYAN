using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // ADICIONADO: Necessário para detetar a UI

namespace InteriorPlanner.Systems.Camera
{
    /// <summary>
    /// Controlador principal da câmara para utilizadores de PC (Teclado e Rato).
    /// Alterna dinamicamente entre dois modos de visualização (Exploração e Edição),
    /// ajustando a física de movimento, velocidade e perspetiva.
    /// </summary>
    public class PlannerCameraController : MonoBehaviour
    {
        // Máquina de estados simples para definir a perspetiva atual do utilizador
        private enum CameraMode
        {
            Exploration, // Modo de primeira pessoa (andar pela sala)
            Edit         // Modo de arquiteto (visão mais alta e rápida)
        }

        [Header("Mode")]
        [SerializeField] private CameraMode currentMode = CameraMode.Exploration;

        [Header("Exploration Settings")]
        [SerializeField] private float explorationMoveSpeed = 4f;
        [SerializeField] private float explorationLookSensitivity = 2f;
        [SerializeField] private float explorationVerticalSpeed = 3f;
        [SerializeField] private float minExplorationY = 1f; // Impede que a câmara atravesse o chão

        [Header("Edit Settings")]
        [SerializeField] private float editMoveSpeed = 8f;
        [SerializeField] private float editLookSensitivity = 2f;
        [SerializeField] private float editVerticalSpeed = 5f;
        [SerializeField] private float editScrollSpeed = 10f;
        [SerializeField] private float minEditY = 2f; // Mantém a câmara acima dos móveis no modo de edição

        [Header("Rotation Limits")]
        [SerializeField] private float minPitch = -80f; // Limite para olhar para baixo (evita cambalhotas da câmara)
        [SerializeField] private float maxPitch = 80f;  // Limite para olhar para cima

        // Armazenam a rotação atual nos eixos Y (Yaw/Esquerda-Direita) e X (Pitch/Cima-Baixo)
        private float yaw;
        private float pitch;

        private void Start()
        {
            // Captura a rotação inicial do objeto no Unity para que a câmara não dê um "salto" no primeiro clique
            Vector3 currentEuler = transform.rotation.eulerAngles;
            yaw = currentEuler.y;
            pitch = NormalizeAngle(currentEuler.x);

            // Ajusta a altura da câmara imediatamente de acordo com o modo inicial escolhido
            ApplyModeStartAdjustment();
        }

        private void Update()
        {
            // Prevenção de crash: Se não houver rato ou teclado ligados, o script pausa
            if (Keyboard.current == null || Mouse.current == null)
                return;

            HandleModeSwitch();

            // Delega o processamento do movimento e rotação de acordo com o estado atual
            if (currentMode == CameraMode.Exploration)
            {
                HandleExplorationMode();
            }
            else
            {
                HandleEditMode();
            }
        }

        /// <summary>
        /// Ouve a tecla TAB para alternar instantaneamente entre a visão de edição e exploração.
        /// </summary>
        private void HandleModeSwitch()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                currentMode = currentMode == CameraMode.Exploration
                    ? CameraMode.Edit
                    : CameraMode.Exploration;

                ApplyModeStartAdjustment();
            }
        }

        private void HandleExplorationMode()
        {
            HandleRotation(explorationLookSensitivity);
            HandleMovement(explorationMoveSpeed, explorationVerticalSpeed, minExplorationY, allowScrollForwardMovement: false);
        }

        private void HandleEditMode()
        {
            HandleRotation(editLookSensitivity);
            HandleMovement(editMoveSpeed, editVerticalSpeed, minEditY, allowScrollForwardMovement: true);
        }

        /// <summary>
        /// Processa o movimento do rato (quando o botão direito está pressionado) 
        /// para calcular a nova rotação da câmara usando Quaternions (para evitar Gimbal Lock).
        /// </summary>
        private void HandleRotation(float sensitivity)
        {
            if (!Mouse.current.rightButton.isPressed)
                return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Multiplica pela sensibilidade e deltaTime para manter a suavidade independente dos FPS (Frames Per Second)
            yaw += mouseDelta.x * sensitivity * Time.deltaTime * 60f;
            pitch -= mouseDelta.y * sensitivity * Time.deltaTime * 60f;
            
            // Tranca o eixo vertical para o utilizador não partir o pescoço
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        /// <summary>
        /// Lê as teclas WASD e Q/E e move a câmara no espaço 3D usando vetores normais.
        /// </summary>
        private void HandleMovement(float moveSpeed, float verticalSpeed, float minY, bool allowScrollForwardMovement)
        {
            Vector3 move = Vector3.zero;

            // Movimento Horizontal e Profundidade
            if (Keyboard.current.wKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed) move += transform.right;

            // Anula o movimento no eixo Y provocado pelo transform.forward (para o "W" não enterrar o utilizador no chão se ele olhar para baixo)
            move.y = 0f;

            // Movimento Vertical dedicado (Elevador)
            if (Keyboard.current.qKey.isPressed) move += Vector3.down * verticalSpeed;
            if (Keyboard.current.eKey.isPressed) move += Vector3.up * verticalSpeed;

            // Aplica a translação física
            if (move != Vector3.zero)
            {
                // move.normalized garante que andar na diagonal não seja mais rápido do que andar em frente
                transform.position += move.normalized * moveSpeed * Time.deltaTime;
            }

            // --- ALTERAÇÃO AQUI: Só faz Zoom se tiver permissão E não estiver sobre a UI ---
            // Proteção crucial: Impede que a câmara faça zoom no mapa quando o utilizador 
            // está apenas a fazer scroll no catálogo de móveis da Interface.
            if (allowScrollForwardMovement && !IsPointerOverUI())
            {
                float scroll = Mouse.current.scroll.ReadValue().y;

                if (Mathf.Abs(scroll) > 0.01f)
                {
                    transform.position += transform.forward * (scroll * 0.01f) * editScrollSpeed;
                }
            }

            // Pós-processamento de colisão no solo
            Vector3 pos = transform.position;
            if (pos.y < minY)
                pos.y = minY;

            transform.position = pos;
        }

        /// <summary>
        /// Faz o "teletransporte" da altura da câmara na transição de modos para garantir
        /// que o utilizador está sempre com a melhor vista assim que carrega no TAB.
        /// </summary>
        private void ApplyModeStartAdjustment()
        {
            if (currentMode == CameraMode.Edit)
            {
                if (transform.position.y < 6f)
                {
                    transform.position = new Vector3(transform.position.x, 6f, transform.position.z - 2f);
                }
            }
            else
            {
                if (transform.position.y > 3f)
                {
                    transform.position = new Vector3(transform.position.x, 1.7f, transform.position.z);
                }
            }
        }

        /// <summary>
        /// Função matemática que normaliza ângulos que passem dos 360 ou desçam abaixo dos -360 graus,
        /// garantindo que a matemática do Pitch (Olhar Cima/Baixo) funciona perfeitamente.
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        // --- NOVA FUNÇÃO ---
        /// <summary>
        /// Interroga o EventSystem da Unity para saber se o cursor do rato 
        /// está fisicamente sobrepondo algum painel, botão ou menu de UI.
        /// </summary>
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}