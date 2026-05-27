using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // ADICIONADO: Necessário para detetar a UI

namespace InteriorPlanner.Systems.Camera
{
    public class PlannerCameraController : MonoBehaviour
    {
        private enum CameraMode
        {
            Exploration,
            Edit
        }

        [Header("Mode")]
        [SerializeField] private CameraMode currentMode = CameraMode.Exploration;

        [Header("Exploration Settings")]
        [SerializeField] private float explorationMoveSpeed = 4f;
        [SerializeField] private float explorationLookSensitivity = 2f;
        [SerializeField] private float explorationVerticalSpeed = 3f;
        [SerializeField] private float minExplorationY = 1f;

        [Header("Edit Settings")]
        [SerializeField] private float editMoveSpeed = 8f;
        [SerializeField] private float editLookSensitivity = 2f;
        [SerializeField] private float editVerticalSpeed = 5f;
        [SerializeField] private float editScrollSpeed = 10f;
        [SerializeField] private float minEditY = 2f;

        [Header("Rotation Limits")]
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private float yaw;
        private float pitch;

        private void Start()
        {
            Vector3 currentEuler = transform.rotation.eulerAngles;
            yaw = currentEuler.y;
            pitch = NormalizeAngle(currentEuler.x);

            ApplyModeStartAdjustment();
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null)
                return;

            HandleModeSwitch();

            if (currentMode == CameraMode.Exploration)
            {
                HandleExplorationMode();
            }
            else
            {
                HandleEditMode();
            }
        }

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

        private void HandleRotation(float sensitivity)
        {
            if (!Mouse.current.rightButton.isPressed)
                return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * sensitivity * Time.deltaTime * 60f;
            pitch -= mouseDelta.y * sensitivity * Time.deltaTime * 60f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void HandleMovement(float moveSpeed, float verticalSpeed, float minY, bool allowScrollForwardMovement)
        {
            Vector3 move = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) move += transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= transform.right;
            if (Keyboard.current.dKey.isPressed) move += transform.right;

            move.y = 0f;

            if (Keyboard.current.qKey.isPressed) move += Vector3.down * verticalSpeed;
            if (Keyboard.current.eKey.isPressed) move += Vector3.up * verticalSpeed;

            if (move != Vector3.zero)
            {
                transform.position += move.normalized * moveSpeed * Time.deltaTime;
            }

            // --- ALTERAÇÃO AQUI: Só faz Zoom se tiver permissão E não estiver sobre a UI ---
            if (allowScrollForwardMovement && !IsPointerOverUI())
            {
                float scroll = Mouse.current.scroll.ReadValue().y;

                if (Mathf.Abs(scroll) > 0.01f)
                {
                    transform.position += transform.forward * (scroll * 0.01f) * editScrollSpeed;
                }
            }

            Vector3 pos = transform.position;
            if (pos.y < minY)
                pos.y = minY;

            transform.position = pos;
        }

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

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        // --- NOVA FUNÇÃO ---
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}