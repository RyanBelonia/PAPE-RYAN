using UnityEngine;

namespace InteriorPlanner.Systems.FloorPlan
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    public class WallLineVisual : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private BoxCollider boxCollider;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            boxCollider = GetComponent<BoxCollider>();

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
        }

        public void SetPoints(Vector3 startPoint, Vector3 endPoint)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);

            UpdateCollider(startPoint, endPoint);
        }

        private void UpdateCollider(Vector3 start, Vector3 end)
        {
            Vector3 center = (start + end) / 2f;
            float length = Vector3.Distance(start, end);

            transform.position = center;
            transform.rotation = Quaternion.LookRotation(end - start);

            boxCollider.size = new Vector3(0.5f, 0.2f, length);
        }
    }
}