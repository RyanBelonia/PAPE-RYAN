using UnityEngine;

namespace InteriorPlanner.Systems.FloorPlan
{
    [RequireComponent(typeof(LineRenderer))]
    public class WallLineVisual : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
        }

        public void SetPoints(Vector3 startPoint, Vector3 endPoint)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }
}