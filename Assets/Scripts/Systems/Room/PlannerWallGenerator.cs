using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Room
{
    public class PlannerWallGenerator : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform wallRoot;
        [SerializeField] private Transform floorRoot;

        [Header("Fallback Settings")]
        [SerializeField] private float fallbackWallHeight = 2.8f;
        [SerializeField] private float fallbackWallThickness = 0.15f;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;

        [Header("Floor Settings")]
        [SerializeField] private float floorThickness = 0.1f;
        [SerializeField] private float floorPadding = 0.2f;

        private void Start()
        {
            GenerateRoomFromCurrentProject();
        }

        private void GenerateRoomFromCurrentProject()
        {
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
            {
                Debug.LogWarning("Nenhum projeto carregado.");
                return;
            }

            ProjectData project = AppManager.Instance.ProjectSession.CurrentProject;

            if (project == null || project.FloorPlan == null || project.FloorPlan.Walls == null || project.FloorPlan.Walls.Count == 0)
            {
                Debug.LogWarning("FloorPlan inválido ou vazio.");
                return;
            }

            ClearExistingGeometry();

            float wallHeight = project.FloorPlan.WallHeight > 0.1f ? project.FloorPlan.WallHeight : fallbackWallHeight;
            float wallThickness = project.FloorPlan.WallThickness > 0.01f ? project.FloorPlan.WallThickness : fallbackWallThickness;

            for (int i = 0; i < project.FloorPlan.Walls.Count; i++)
            {
                CreateWall(project.FloorPlan.Walls[i], wallHeight, wallThickness);
            }

            CreateAutomaticFloor(project.FloorPlan);
        }

        private void CreateWall(WallSegmentData wallSegment, float wallHeight, float wallThickness)
        {
            Vector3 start = new Vector3(wallSegment.StartPoint.x, 0f, wallSegment.StartPoint.y);
            Vector3 end = new Vector3(wallSegment.EndPoint.x, 0f, wallSegment.EndPoint.y);

            Vector3 direction = end - start;
            float length = direction.magnitude;

            if (length <= 0.001f)
                return;

            Vector3 center = (start + end) / 2f;
            center.y = wallHeight / 2f;

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall_3D";
            wall.transform.SetParent(wallRoot, true);

            wall.transform.position = center;
            wall.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            wall.transform.localScale = new Vector3(wallThickness, wallHeight, length);

            if (wallMaterial != null)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                renderer.material = wallMaterial;
            }
        }

        private void CreateAutomaticFloor(FloorPlanData floorPlan)
        {
            if (floorPlan == null || floorPlan.Walls == null || floorPlan.Walls.Count == 0)
                return;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < floorPlan.Walls.Count; i++)
            {
                WallSegmentData wall = floorPlan.Walls[i];

                UpdateBounds(wall.StartPoint.x, wall.StartPoint.y, ref minX, ref maxX, ref minZ, ref maxZ);
                UpdateBounds(wall.EndPoint.x, wall.EndPoint.y, ref minX, ref maxX, ref minZ, ref maxZ);
            }

            float width = (maxX - minX) + floorPadding * 2f;
            float length = (maxZ - minZ) + floorPadding * 2f;

            Vector3 center = new Vector3(
                (minX + maxX) / 2f,
                -floorThickness / 2f,
                (minZ + maxZ) / 2f
            );

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_3D";
            floor.transform.SetParent(floorRoot, true);
            floor.transform.position = center;
            floor.transform.localScale = new Vector3(width, floorThickness, length);

            if (floorMaterial != null)
            {
                Renderer renderer = floor.GetComponent<Renderer>();
                renderer.material = floorMaterial;
            }
        }

        private void UpdateBounds(float x, float z, ref float minX, ref float maxX, ref float minZ, ref float maxZ)
        {
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }

        private void ClearExistingGeometry()
        {
            ClearChildren(wallRoot);
            ClearChildren(floorRoot);
        }

        private void ClearChildren(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}