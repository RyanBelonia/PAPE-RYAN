using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using InteriorPlanner.Core; // Adicionado para acessar o AppManager
using InteriorPlanner.Data; // Adicionado para acessar o RoomData

namespace InteriorPlanner.Systems.Placement
{
    public class ObjectMover : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask floorLayerMask; 
        [SerializeField] private SelectionManager selectionManager;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeedScroll = 15f; 
        [SerializeField] private float rotationSpeedKey = 45f;    

        [Header("Placement Settings")]
        [SerializeField] private bool useGridSnap = false; 
        [SerializeField] private float gridSize = 0.5f; 
        [SerializeField] private bool useSmoothMovement = true; 
        [SerializeField] private float smoothSpeed = 15f;

        private PlaceableObject currentObject;
        private bool isDragging;
        private Vector3 dragOffset; 

        private void LateUpdate()
        {
            if (Mouse.current == null || Keyboard.current == null || mainCamera == null || selectionManager == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;

                currentObject = selectionManager.GetSelectedObject();

                if (currentObject != null && currentObject.CanMove)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                    {
                        dragOffset = currentObject.transform.position - hit.point;
                        dragOffset.y = 0; 
                        isDragging = true;
                    }
                }
            }

            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                if (currentObject == null)
                {
                    isDragging = false;
                    return;
                }

                MoveObjectWithMouse();
                RotateObjectWithInput();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
                currentObject = null;
            }
        }

        private void MoveObjectWithMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
            {
                Vector3 targetPosition = hit.point + dragOffset;
                targetPosition.y = currentObject.transform.position.y;

                if (useGridSnap)
                {
                    targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                    targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
                }

                // --- NOVO: RESTRINGIR AOS LIMITES DA SALA ---
                targetPosition = ClampPositionToRoom(targetPosition);

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

        // --- NOVA FUNÇÃO ---
        private Vector3 ClampPositionToRoom(Vector3 targetPos)
        {
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
                return targetPos;

            RoomData room = AppManager.Instance.ProjectSession.CurrentProject.Room;
            if (room == null) return targetPos;

            // Pega o tamanho real do móvel rodado no mundo
            Collider col = currentObject.GetComponentInChildren<Collider>();
            float extentsX = col != null ? col.bounds.extents.x : 0f;
            float extentsZ = col != null ? col.bounds.extents.z : 0f;

            // Calcula as paredes baseando-se no tamanho da sala e desconta metade do móvel
            float minX = (-room.Width / 2f) + extentsX;
            float maxX = (room.Width / 2f) - extentsX;
            
            float minZ = (-room.Length / 2f) + extentsZ;
            float maxZ = (room.Length / 2f) - extentsZ;

            // Aplica a barreira invisível
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

            return targetPos;
        }

        private void RotateObjectWithInput()
        {
            if (!currentObject.CanRotate) return; 

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                currentObject.transform.Rotate(Vector3.up, rotationSpeedKey);
            }

            float scrollValue = Mouse.current.scroll.y.ReadValue();
            if (scrollValue > 0)
            {
                currentObject.transform.Rotate(Vector3.up, rotationSpeedScroll);
            }
            else if (scrollValue < 0)
            {
                currentObject.transform.Rotate(Vector3.up, -rotationSpeedScroll);
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}