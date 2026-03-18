using System.Collections.Generic;
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
        [SerializeField] private Transform vertexRoot;

        [Header("Drawing Setup")]
        [SerializeField] private LayerMask drawingLayerMask;
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField] private GameObject wallLinePrefab;
        [SerializeField] private GameObject vertexPointPrefab;
        [SerializeField] private float lineYPosition = 0.05f;
        [SerializeField] private float vertexYPosition = 0.06f;

        [Header("Vertex Settings")]
        [SerializeField] private float vertexDuplicateTolerance = 0.01f;

        private FloorPlanData currentFloorPlan;
        private Vector3? lastPlacedPoint;
        private GameObject previewLineObject;
        private WallLineVisual previewLineVisual;

        private readonly List<Vector3> placedVertices = new();

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
                bool ctrlPressed =
                    Keyboard.current != null &&
                    (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

                if (ctrlPressed)
                {
                    TryDeleteWall();
                }
                else
                {
                    HandleLeftClick();
                }
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
                CreateVertexIfNeeded(clickedPoint);
                return;
            }

            Vector3 startPoint = lastPlacedPoint.Value;
            Vector3 endPoint = clickedPoint;

            if (Vector3.Distance(startPoint, endPoint) <= 0.01f)
                return;

            CreateWallSegment(startPoint, endPoint);
            CreateVertexIfNeeded(endPoint);

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

        private void CreateVertexIfNeeded(Vector3 point)
        {
            if (vertexPointPrefab == null || vertexRoot == null)
                return;

            Vector3 vertexPosition = new Vector3(point.x, vertexYPosition, point.z);

            for (int i = 0; i < placedVertices.Count; i++)
            {
                if (Vector3.Distance(placedVertices[i], vertexPosition) <= vertexDuplicateTolerance)
                {
                    return;
                }
            }

            Instantiate(vertexPointPrefab, vertexPosition, Quaternion.identity, vertexRoot);
            placedVertices.Add(vertexPosition);
        }

        private void TryDeleteWall()
        {
            if (Mouse.current == null)
                return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = editorCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, wallLayerMask))
            {
                WallLineVisual wall = hit.collider.GetComponent<WallLineVisual>();

                if (wall != null)
                {
                    RemoveWallData(wall);
                    Destroy(wall.gameObject);
                    RebuildVertices();
                }
            }
        }

        private void RemoveWallData(WallLineVisual wall)
        {
            if (wall == null || currentFloorPlan == null)
                return;

            Vector3 start = wall.GetStartPoint();
            Vector3 end = wall.GetEndPoint();

            for (int i = currentFloorPlan.Walls.Count - 1; i >= 0; i--)
            {
                WallSegmentData segment = currentFloorPlan.Walls[i];

                Vector2 segStart = segment.StartPoint;
                Vector2 segEnd = segment.EndPoint;

                bool sameDirection =
                    Vector2.Distance(segStart, new Vector2(start.x, start.z)) < 0.01f &&
                    Vector2.Distance(segEnd, new Vector2(end.x, end.z)) < 0.01f;

                bool oppositeDirection =
                    Vector2.Distance(segStart, new Vector2(end.x, end.z)) < 0.01f &&
                    Vector2.Distance(segEnd, new Vector2(start.x, start.z)) < 0.01f;

                if (sameDirection || oppositeDirection)
                {
                    currentFloorPlan.Walls.RemoveAt(i);
                    break;
                }
            }
        }

        private void RebuildVertices()
        {
            ClearAllVertices();

            if (currentFloorPlan == null)
                return;

            for (int i = 0; i < currentFloorPlan.Walls.Count; i++)
            {
                WallSegmentData wall = currentFloorPlan.Walls[i];

                Vector3 start = new Vector3(wall.StartPoint.x, lineYPosition, wall.StartPoint.y);
                Vector3 end = new Vector3(wall.EndPoint.x, lineYPosition, wall.EndPoint.y);

                CreateVertexIfNeeded(start);
                CreateVertexIfNeeded(end);
            }
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

        public void OnClickClearPlan()
        {
            ClearAllWalls();
            ClearAllVertices();
            CancelCurrentChain();

            if (currentFloorPlan != null)
            {
                currentFloorPlan.Walls.Clear();
            }
        }

        public void OnClickFinishDrawing()
        {
            CancelCurrentChain();
        }

        public void OnClickBackToMenu()
        {
            SceneController.LoadMainMenu();
        }

        private void ClearAllWalls()
        {
            for (int i = drawingRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(drawingRoot.GetChild(i).gameObject);
            }
        }

        private void ClearAllVertices()
        {
            placedVertices.Clear();

            if (vertexRoot == null)
                return;

            for (int i = vertexRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(vertexRoot.GetChild(i).gameObject);
            }
        }
    }
}