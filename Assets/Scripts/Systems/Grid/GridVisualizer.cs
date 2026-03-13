using UnityEngine;

//teste 
namespace InteriorPlanner.Systems.Grid
{
    public class GridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;

        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 0.5f;
        [SerializeField] private float yPosition = 0.01f;
        [SerializeField] private float lineWidth = 0.02f;
        [SerializeField] private Material lineMaterial;

        [Header("Extra Coverage")]
        [SerializeField] private int extraLines = 2;

        private float lastOrthographicSize;
        private float lastAspect;

        private void Start()
        {
            GenerateGrid();
            SaveCameraState();
        }

        private void Update()
        {
            if (targetCamera == null || !targetCamera.orthographic)
                return;

            if (!Mathf.Approximately(lastOrthographicSize, targetCamera.orthographicSize) ||
                !Mathf.Approximately(lastAspect, targetCamera.aspect))
            {
                GenerateGrid();
                SaveCameraState();
            }
        }

        private void SaveCameraState()
        {
            if (targetCamera == null)
                return;

            lastOrthographicSize = targetCamera.orthographicSize;
            lastAspect = targetCamera.aspect;
        }

        public void GenerateGrid()
        {
            if (targetCamera == null || !targetCamera.orthographic || cellSize <= 0f)
                return;

            ClearExistingGrid();

            float visibleHalfHeight = targetCamera.orthographicSize;
            float visibleHalfWidth = visibleHalfHeight * targetCamera.aspect;

            float minX = -visibleHalfWidth;
            float maxX = visibleHalfWidth;
            float minZ = -visibleHalfHeight;
            float maxZ = visibleHalfHeight;

            int startXIndex = Mathf.FloorToInt(minX / cellSize) - extraLines;
            int endXIndex = Mathf.CeilToInt(maxX / cellSize) + extraLines;

            int startZIndex = Mathf.FloorToInt(minZ / cellSize) - extraLines;
            int endZIndex = Mathf.CeilToInt(maxZ / cellSize) + extraLines;

            float lineMinX = startXIndex * cellSize;
            float lineMaxX = endXIndex * cellSize;
            float lineMinZ = startZIndex * cellSize;
            float lineMaxZ = endZIndex * cellSize;

            for (int x = startXIndex; x <= endXIndex; x++)
            {
                CreateLine(
                    $"GridLine_V_{x}",
                    new Vector3(x * cellSize, yPosition, lineMinZ),
                    new Vector3(x * cellSize, yPosition, lineMaxZ)
                );
            }

            for (int z = startZIndex; z <= endZIndex; z++)
            {
                CreateLine(
                    $"GridLine_H_{z}",
                    new Vector3(lineMinX, yPosition, z * cellSize),
                    new Vector3(lineMaxX, yPosition, z * cellSize)
                );
            }
        }

        private void CreateLine(string lineName, Vector3 start, Vector3 end)
        {
            GameObject lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = lineMaterial;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
        }

        private void ClearExistingGrid()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}