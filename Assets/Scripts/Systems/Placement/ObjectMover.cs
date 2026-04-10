using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace InteriorPlanner.Systems.Placement
{
    public class ObjectMover : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private LayerMask floorLayerMask; // TEM DE ESTAR APONTADO PARA "Ground"
        [SerializeField] private SelectionManager selectionManager;

        private PlaceableObject currentObject;
        private bool isDragging;
        private Vector3 dragOffset; // O segredo para o objeto não "teleportar"

        // Usamos LateUpdate para dar tempo ao SelectionManager de selecionar o objeto no Update()
        private void LateUpdate()
        {
            if (Mouse.current == null || mainCamera == null || selectionManager == null)
                return;

            // 1. COMEÇAR DRAG (Momento do Clique)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;

                currentObject = selectionManager.GetSelectedObject();

                // Só começa o drag se clicámos num objeto válido e que se pode mover
                if (currentObject != null && currentObject.CanMove)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    
                    // Dispara o raio contra o chão para saber onde o rato tocou inicialmente
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
                    {
                        // Calcula a diferença entre a posição do objeto e onde o rato bateu no chão
                        dragOffset = currentObject.transform.position - hit.point;
                        dragOffset.y = 0; // Ignoramos o Y para a altura não se descontrolar
                        isDragging = true;
                    }
                }
            }

            // 2. DURANTE DRAG (Segurar o botão)
            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                if (currentObject == null)
                {
                    isDragging = false;
                    return;
                }

                MoveObjectWithMouse();
            }

            // 3. TERMINAR DRAG (Largar o botão)
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
                currentObject = null;
            }
        }

        private void MoveObjectWithMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Só move se o rato estiver a apontar para o chão
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, floorLayerMask))
            {
                // A nova posição é onde o rato está + a distância inicial (Offset)
                Vector3 targetPosition = hit.point + dragOffset;

                // Força a altura original do objeto para ele não afundar no chão
                targetPosition.y = currentObject.transform.position.y;

                currentObject.transform.position = targetPosition;
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}