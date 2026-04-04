using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance { get; private set; }

        [SerializeField] private Transform furnitureRoot;
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private float spawnDistanceFromCamera = 3f;
        [SerializeField] private float defaultYOffset = 0f;
        [SerializeField] private float defaultFurnitureScale = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void PlaceFurniture(FurnitureItemData item)
        {
            if (item == null || item.Prefab == null)
            {
                Debug.LogWarning("Furniture inválido ou prefab não atribuído.");
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("Main Camera não atribuída no FurniturePlacer.");
                return;
            }

            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnDistanceFromCamera;
            spawnPosition.y = defaultYOffset;

            GameObject furniture = Instantiate(item.Prefab, spawnPosition, Quaternion.identity, furnitureRoot);
            furniture.name = item.DisplayName;
            furniture.transform.localScale = Vector3.one * defaultFurnitureScale;
        }
    }
}