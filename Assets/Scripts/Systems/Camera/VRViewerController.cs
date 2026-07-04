using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace InteriorPlanner.Systems.VR
{
    /// <summary>
    /// Controlador independente para os óculos de Realidade Virtual (Meta Quest / OpenXR).
    /// Isola a lógica de inputs do VR (comandos) da lógica de PC para não causar conflitos,
    /// permitindo a navegação imersiva dentro da estrutura desenhada pelo utilizador.
    /// </summary>
    public class VRViewerController : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Arrasta a tua Main Camera para aqui")]
        [SerializeField] private Transform vrCamera;

        [Header("Velocidades")]
        public float moveSpeed = 3f;      // Velocidade de andar (Mão Direita)
        public float verticalSpeed = 2f;  // Velocidade de subir/descer (Mão Esquerda Y)
        public float turnSpeed = 60f;     // Velocidade de rodar a câmara (Mão Esquerda X)

        // Variáveis que armazenam as referências de hardware dos comandos físicos
        private InputDevice rightController;
        private InputDevice leftController;

        private void Start()
        {
            // Tenta estabelecer a ponte com a API do OpenXR logo no arranque
            ConnectControllers();
        }

        private void Update()
        {
            // Gestão de Ciclo de Vida: Se os comandos adormecerem (standby) para poupar bateria 
            // ou se o utilizador ligar os óculos depois da aplicação já estar a correr, 
            // o sistema tenta emparelhar novamente as variáveis a cada frame.
            if (!rightController.isValid || !leftController.isValid)
            {
                ConnectControllers();
            }

            HandleRightJoystick();
            HandleLeftJoystick();
        }

        /// <summary>
        /// Processa a locomoção espacial 2D (Frente/Trás/Esquerda/Direita).
        /// </summary>
        private void HandleRightJoystick()
        {
            // MÃO DIREITA: Movimento horizontal (Frente/Trás/Lados) lendo o Vector2 do analógico
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStick))
            {
                // Lógica de Deslocamento Relativo: 
                // A frente do jogador não é o "Norte" da sala, é o ponto para onde os olhos (óculos) estão a apontar.
                Vector3 forward = vrCamera.forward;
                Vector3 right = vrCamera.right;

                // Anula o eixo Y da direção:
                // Previne o erro comum em VR em que olhar para o teto e andar para a frente 
                // faria o jogador descolar do chão e voar pela sala.
                forward.y = 0;
                right.y = 0;
                
                // Normaliza para manter o vetor com tamanho igual a 1 (para manter a velocidade constante)
                forward.Normalize();
                right.Normalize();

                // Multiplica a direção pela inclinação do analógico e soma as componentes X e Y
                Vector3 moveDir = (right * rightStick.x) + (forward * rightStick.y);
                
                // Aplica a translação no Transform "Pai" (VR_Rig) que arrastará a câmara de forma invisível
                transform.position += moveDir * (moveSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Processa a elevação (Y) e a rotação em torno do próprio eixo, 
        /// evitando que o utilizador precise de girar fisicamente o corpo num espaço pequeno.
        /// </summary>
        private void HandleLeftJoystick()
        {
            // MÃO ESQUERDA: Vertical (Y do analógico) e Rotação (X do analógico)
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStick))
            {
                // Sobe e desce a altura do jogador (Eixo Y do analógico) - Efeito Elevador
                transform.position += Vector3.up * (leftStick.y * verticalSpeed * Time.deltaTime);

                // Roda o jogador no eixo Y para olhar para os lados (Eixo X do analógico)
                transform.Rotate(0, leftStick.x * turnSpeed * Time.deltaTime, 0);
            }
        }

        /// <summary>
        /// Comunica com a camada base do Unity XR para descobrir e guardar os periféricos do utilizador.
        /// </summary>
        private void ConnectControllers()
        {
            List<InputDevice> devices = new List<InputDevice>();

            // Procura todos os dispositivos fisicamente presentes na mão direita do jogador
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0) rightController = devices[0];

            // Procura todos os dispositivos fisicamente presentes na mão esquerda
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0) leftController = devices[0];
        }
    }
}