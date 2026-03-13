using UnityEngine;
using UnityEngine.InputSystem;

namespace InteriorPlanner.Utilities
{
    public static class MouseWorldUtility
    {
        public static bool TryGetMousePositionOnPlane(Camera camera, LayerMask layerMask, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            if (camera == null || Mouse.current == null)
                return false;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask))
            {
                hitPoint = hit.point;
                return true;
            }

            return false;
        }
    }
}