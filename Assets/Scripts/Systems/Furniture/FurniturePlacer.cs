using UnityEngine;
using InteriorPlanner.Systems.Placement; // ADICIONADO: Para aceder ao SelectionManager e PlaceableObject

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

        // ADICIONADO: Referência para o nosso SelectionManager
        [SerializeField] private SelectionManager selectionManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Se esqueceres de arrastar no Inspector, ele procura automaticamente!
            if (selectionManager == null)
            {
                selectionManager = FindObjectOfType<SelectionManager>();
            }
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

            // Posição inicial (vai ser substituída se for uma janela/porta)
            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnDistanceFromCamera;
            spawnPosition.y = defaultYOffset;

            // 1. Cria o objeto
            GameObject furniture = Instantiate(item.Prefab, spawnPosition, Quaternion.identity, furnitureRoot);
            furniture.name = item.DisplayName;
            furniture.transform.localScale = Vector3.one * defaultFurnitureScale;

            // --- A MAGIA ACONTECE AQUI ---
            // 2. Tenta encontrar a inteligência (PlaceableObject) do modelo que acabámos de criar
            PlaceableObject placeable = furniture.GetComponent<PlaceableObject>();
            
            if (placeable != null && selectionManager != null)
            {
                // 3. Diz ao SelectionManager: "Toma este objeto! Seleciona-o e, se for de parede, cola-o!"
                selectionManager.ForceSelectAndSnap(placeable);
            }
        }
    }
}