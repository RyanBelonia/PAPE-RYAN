using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Room
{
    public class RoomGenerator : MonoBehaviour
    {
        [SerializeField] private Transform roomRoot;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private float wallThickness = 0.15f;
        [SerializeField] private float floorThickness = 0.1f;

        private void Start()
        {
            GenerateRoom();
        }

        private void GenerateRoom()
        {
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
            {
                Debug.LogWarning("Nenhum projeto carregado.");
                return;
            }

            RoomData room = AppManager.Instance.ProjectSession.CurrentProject.Room;

            if (room == null)
            {
                Debug.LogWarning("RoomData inválido.");
                return;
            }

            ClearRoom();

            CreateFloor(room);
            CreateWalls(room);
        }

        private void CreateFloor(RoomData room)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(roomRoot, true);
            floor.transform.position = new Vector3(0f, -floorThickness / 2f, 0f);
            floor.transform.localScale = new Vector3(room.Width, floorThickness, room.Length);

            if (floorMaterial != null)
                floor.GetComponent<Renderer>().material = floorMaterial;
        }

        private void CreateWalls(RoomData room)
        {
            CreateWall(
                "Wall_Front",
                new Vector3(0f, room.Height / 2f, room.Length / 2f),
                new Vector3(room.Width, room.Height, wallThickness)
            );

            CreateWall(
                "Wall_Back",
                new Vector3(0f, room.Height / 2f, -room.Length / 2f),
                new Vector3(room.Width, room.Height, wallThickness)
            );

            CreateWall(
                "Wall_Left",
                new Vector3(-room.Width / 2f, room.Height / 2f, 0f),
                new Vector3(wallThickness, room.Height, room.Length)
            );

            CreateWall(
                "Wall_Right",
                new Vector3(room.Width / 2f, room.Height / 2f, 0f),
                new Vector3(wallThickness, room.Height, room.Length)
            );
        }

        private void CreateWall(string wallName, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(roomRoot, true);
            wall.transform.position = position;
            wall.transform.localScale = scale;

            if (wallMaterial != null)
                wall.GetComponent<Renderer>().material = wallMaterial;
        }

        private void ClearRoom()
        {
            for (int i = roomRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(roomRoot.GetChild(i).gameObject);
            }
        }
    }
} 