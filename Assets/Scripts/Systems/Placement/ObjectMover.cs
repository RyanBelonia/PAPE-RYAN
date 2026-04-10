using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
        [SerializeField] private bool useGridSnap = false; // Liga no Inspector para usar grelha!
        [SerializeField] private float gridSize = 0.5f; // Tamanho da grelha (ex: 0.5 metros)
        [SerializeField] private bool useSmoothMovement = true; // Deixa o movimento suave
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
                // Posição base (Rato + Offset)
                Vector3 targetPosition = hit.point + dragOffset;
                targetPosition.y = currentObject.transform.position.y;

                // --- SISTEMA DE GRELHA (GRID SNAP) ---
                if (useGridSnap)
                {
                    targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                    targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
                }

                // --- SISTEMA DE MOVIMENTO ---
                if (useSmoothMovement && !useGridSnap) 
                {
                    // Movimento suave (escorrega até ao rato)
                    currentObject.transform.position = Vector3.Lerp(currentObject.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
                }
                else
                {
                    // Movimento instantâneo (obrigatório se estivermos a usar a grelha)
                    currentObject.transform.position = targetPosition;
                }
            }
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