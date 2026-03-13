using UnityEngine;
using UnityEngine.InputSystem;
using InteriorPlanner.Core;
using InteriorPlanner.Data;
using InteriorPlanner.Utilities;

namespace InteriorPlanner.Systems.FloorPlan
{
    public class FloorPlanEditorController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Camera editorCamera;
        [SerializeField] private Transform drawingRoot;
        [SerializeField] private Transform previewRoot;

        [Header("Drawing Setup")]
        [SerializeField] private LayerMask drawingLayerMask;
        [SerializeField] private GameObject wallLinePrefab;
        [SerializeField] private float lineYPosition = 0.05f;

        private FloorPlanData currentFloorPlan;
        private Vector3? lastPlacedPoint;
        private GameObject previewLineObject;
        private WallLineVisual previewLineVisual;

        private void Start()
        {
            InitializeFloorPlan();
            CreatePreviewLine();
        }

        private void Update()
        {
            HandlePreview();

            if (Mouse.current == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleLeftClick();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelCurrentChain();
            }
        }

        private void InitializeFloorPlan()
        {
            currentFloorPlan = new FloorPlanData();

            if (AppManager.Instance != null)
            {
                AppManager.Instance.ProjectSession.CreateNewProject(currentFloorPlan);
            }
            else
            {
                Debug.LogWarning("AppManager não encontrado. A planta atual não será guardada na sessão.");
            }
        }

        private void HandleLeftClick()
        {
            if (!MouseWorldUtility.TryGetMousePositionOnPlane(editorCamera, drawingLayerMask, out Vector3 hitPoint))
                return;

            Vector3 clickedPoint = GetFlatPoint(hitPoint);

            if (!lastPlacedPoint.HasValue)
            {
                lastPlacedPoint = clickedPoint;
                return;
            }

            Vector3 startPoint = lastPlacedPoint.Value;
            Vector3 endPoint = clickedPoint;

            if (Vector3.Distance(startPoint, endPoint) <= 0.01f)
                return;

            CreateWallSegment(startPoint, endPoint);
            lastPlacedPoint = endPoint;
        }

        private void HandlePreview()
        {
            if (!lastPlacedPoint.HasValue || previewLineVisual == null)
                return;

            if (!MouseWorldUtility.TryGetMousePositionOnPlane(editorCamera, drawingLayerMask, out Vector3 hitPoint))
                return;

            Vector3 previewEndPoint = GetFlatPoint(hitPoint);
            previewLineObject.SetActive(true);
            previewLineVisual.SetPoints(lastPlacedPoint.Value, previewEndPoint);
        }

        private void CreateWallSegment(Vector3 startPoint, Vector3 endPoint)
        {
            WallSegmentData segmentData = new WallSegmentData(
                new Vector2(startPoint.x, startPoint.z),
                new Vector2(endPoint.x, endPoint.z)
            );

            currentFloorPlan.Walls.Add(segmentData);

            GameObject wallObject = Instantiate(wallLinePrefab, drawingRoot);
            WallLineVisual wallVisual = wallObject.GetComponent<WallLineVisual>();
            wallVisual.SetPoints(startPoint, endPoint);
        }

        private Vector3 GetFlatPoint(Vector3 originalPoint)
        {
            return new Vector3(originalPoint.x, lineYPosition, originalPoint.z);
        }

        private void CancelCurrentChain()
        {
            lastPlacedPoint = null;

            if (previewLineObject != null)
            {
                previewLineObject.SetActive(false);
            }
        }

        private void CreatePreviewLine()
        {
            previewLineObject = Instantiate(wallLinePrefab, previewRoot);
            previewLineObject.name = "PreviewWallLine";

            previewLineVisual = previewLineObject.GetComponent<WallLineVisual>();
            previewLineObject.SetActive(false);
        }
    }
}