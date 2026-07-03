using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace InteriorPlanner.Systems.VR
{
    public class VRViewerController : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Arrasta a tua Main Camera para aqui")]
        [SerializeField] private Transform vrCamera;

        [Header("Velocidades")]
        public float moveSpeed = 3f;      // Velocidade de andar (Mão Direita)
        public float verticalSpeed = 2f;  // Velocidade de subir/descer (Mão Esquerda Y)
        public float turnSpeed = 60f;     // Velocidade de rodar a câmara (Mão Esquerda X)

        private InputDevice rightController;
        private InputDevice leftController;

        private void Start()
        {
            ConnectControllers();
        }

        private void Update()
        {
            // Se os comandos adormecerem ou forem ligados depois, tenta ligar novamente
            if (!rightController.isValid || !leftController.isValid)
            {
                ConnectControllers();
            }

            HandleRightJoystick();
            HandleLeftJoystick();
        }

        private void HandleRightJoystick()
        {
            // MÃO DIREITA: Movimento horizontal (Frente/Trás/Lados)
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStick))
            {
                // Calcula a direção baseada para onde a cabeça está a olhar
                Vector3 forward = vrCamera.forward;
                Vector3 right = vrCamera.right;

                // Anula o eixo Y para não voares quando olhas para o teto
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDir = (right * rightStick.x) + (forward * rightStick.y);
                transform.position += moveDir * (moveSpeed * Time.deltaTime);
            }
        }

        private void HandleLeftJoystick()
        {
            // MÃO ESQUERDA: Vertical (Y do analógico) e Rotação (X do analógico)
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStick))
            {
                // Sobe e desce a altura do jogador (Eixo Y do analógico)
                transform.position += Vector3.up * (leftStick.y * verticalSpeed * Time.deltaTime);

                // Roda o jogador no eixo Y para olhar para os lados (Eixo X do analógico)
                transform.Rotate(0, leftStick.x * turnSpeed * Time.deltaTime, 0);
            }
        }

        private void ConnectControllers()
        {
            List<InputDevice> devices = new List<InputDevice>();

            // Procura e liga o comando direito
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0) rightController = devices[0];

            // Procura e liga o comando esquerdo
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0) leftController = devices[0];
        }
    }
}