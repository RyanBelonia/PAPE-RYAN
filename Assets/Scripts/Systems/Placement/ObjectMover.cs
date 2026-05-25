using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Placement
{
    public class ObjectMover : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask floorLayerMask; 
        [SerializeField] private LayerMask obstacleLayerMask; 
        [SerializeField] private LayerMask wallLayerMask;
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
                    
                    // --- ALTERAÇÃO: Offset adaptado para Parede vs Chão ---
                    if (currentObject.RequiresWallSupport)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, 500f, wallLayerMask))
                        {
                            // Para objetos de parede, o offset é calculado em relação à normal da parede
                            dragOffset = currentObject.transform.position - hit.point;
                            isDragging = true;
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                        {
                            dragOffset = currentObject.transform.position - hit.point;
                            dragOffset.y = 0; 
                            isDragging = true;
                        }
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
                CheckCollisions(); 
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (currentObject != null && currentObject.IsValidPosition)
                {
                    isDragging = false;
                    currentObject = null;
                }
            }
        }

        private void MoveObjectWithMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // --- RAMIFICAÇÃO 1: OBJETOS DE PAREDE (Arrastar pela parede) ---
            if (currentObject.RequiresWallSupport)
            {
                // Dispara o raio contra TODAS as paredes para permitir transitar de uma parede para outra
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, wallLayerMask))
                {
                    // Mantém a altura inicial do objeto (para portas não afundarem no chão, por exemplo)
                    // Mas permite mover horizontalmente ao longo da parede
                    Vector3 targetPos = hit.point;
                    
                    // Se quiseres que a janela "siga" o rato em altura livremente, retira a linha abaixo.
                    // Se preferires que ela mantenha a altura a que foi criada (ou de uma porta), mantém.
                    // targetPos.y = currentObject.transform.position.y; 

                    currentObject.transform.position = targetPos;
                    currentObject.transform.rotation = Quaternion.LookRotation(hit.normal);
                }
                // Se o rato sair da parede, não faz nada (o objeto fica colado no último ponto válido)
            }
            // --- RAMIFICAÇÃO 2: OBJETOS DE CHÃO (Móveis / Divisórias) ---
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                {
                    Vector3 targetPosition = hit.point + dragOffset;
                    targetPosition.y = currentObject.transform.position.y;

                    if (useGridSnap)
                    {
                        targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                        targetPosition.z = Mathf.Round(targetPosition.z / gridSize) * gridSize;
                    }

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
        }

        // ... (O resto do teu código ObjectMover continua igual a partir daqui) ...
        private void CheckCollisions()
        {
            BoxCollider boxCol = currentObject.GetComponentInChildren<BoxCollider>();
            if (boxCol == null) return;

            Vector3 worldCenter = boxCol.transform.TransformPoint(boxCol.center);
            Vector3 halfExtents = Vector3.Scale(boxCol.size, boxCol.transform.lossyScale) * 0.5f;
            halfExtents *= 0.95f; 

            Collider[] hits = Physics.OverlapBox(worldCenter, halfExtents, boxCol.transform.rotation, obstacleLayerMask);

            bool isColliding = false;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject != currentObject.gameObject && hit.transform.root != currentObject.transform.root)
                {
                    isColliding = true;
                    break;
                }
            }

            currentObject.SetValidState(!isColliding);
        }

        private Vector3 ClampPositionToRoom(Vector3 targetPos)
        {
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
                return targetPos;

            RoomData room = AppManager.Instance.ProjectSession.CurrentProject.Room;
            if (room == null) return targetPos;

            Collider col = currentObject.GetComponentInChildren<Collider>();
            float extentsX = col != null ? col.bounds.extents.x : 0f;
            float extentsZ = col != null ? col.bounds.extents.z : 0f;

            float minX = (-room.Width / 2f) + extentsX;
            float maxX = (room.Width / 2f) - extentsX;
            
            float minZ = (-room.Length / 2f) + extentsZ;
            float maxZ = (room.Length / 2f) - extentsZ;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

            return targetPos;
        }

        private void RotateObjectWithInput()
        {
            if (!currentObject.CanRotate) return; 

            if (Keyboard.current.rKey.wasPressedThisFrame)
                currentObject.transform.Rotate(Vector3.up, rotationSpeedKey);

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