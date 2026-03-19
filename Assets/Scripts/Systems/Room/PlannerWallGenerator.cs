using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Room
{
    public class PlannerWallGenerator : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform wallRoot;

        [Header("Wall Settings")]
        [SerializeField] private float wallHeight = 2.8f;
        [SerializeField] private float wallThickness = 0.15f;
        [SerializeField] private Material wallMaterial;

        private void Start()
        {
            GenerateWalls();
        }

        private void GenerateWalls()
        {
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
            {
                Debug.LogWarning("Nenhum projeto carregado.");
                return;
            }

            ProjectData project = AppManager.Instance.ProjectSession.CurrentProject;

            if (project == null || project.FloorPlan == null || project.FloorPlan.Walls == null)
            {
                Debug.LogWarning("FloorPlan inválido.");
                return;
            }

            ClearExistingWalls();

            for (int i = 0; i < project.FloorPlan.Walls.Count; i++)
            {
                CreateWall(project.FloorPlan.Walls[i]);
            }
        }

        private void CreateWall(WallSegmentData wallSegment)
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

        private void ClearExistingWalls()
        {
            if (wallRoot == null)
                return;

            for (int i = wallRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(wallRoot.GetChild(i).gameObject);
            }
        }
    }
}