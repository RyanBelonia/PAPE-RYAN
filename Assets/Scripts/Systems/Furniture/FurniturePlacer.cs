using UnityEngine;
using InteriorPlanner.Systems.Placement; // ADICIONADO: Para aceder ao SelectionManager e PlaceableObject

namespace InteriorPlanner.Systems.Furniture
{
    /// <summary>
    /// O motor responsável por "materializar" (instanciar) o objeto 3D no espaço físico 
    /// a partir do botão clicado na UI.
    /// </summary>
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance { get; private set; }

        [SerializeField] private Transform furnitureRoot; // A pasta 3D que vai organizar os móveis gerados
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private float spawnDistanceFromCamera = 3f;
        [SerializeField] private float defaultYOffset = 0f;
        [SerializeField] private float defaultFurnitureScale = 0.5f;

        // Referência essencial para ativar o modo de movimento (Gizmos) imediatamente após a criação
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
            // Sistema de tolerância a falhas: se o programador se esquecer de arrastar 
            // a referência no Inspector, o script procura-a sozinho.
            if (selectionManager == null)
            {
                selectionManager = FindObjectOfType<SelectionManager>();
            }
        }

        /// <summary>
        /// Instancia um clone do modelo 3D à frente da câmara e seleciona-o automaticamente
        /// para o utilizador o colocar no sítio certo.
        /// </summary>
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

            // Posição inicial flutuante (vai ser substituída pela parede se for uma janela/porta)
            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnDistanceFromCamera;
            spawnPosition.y = defaultYOffset;

            // 1. Cria o objeto físico na cena e aplica a escala base
            GameObject furniture = Instantiate(item.Prefab, spawnPosition, Quaternion.identity, furnitureRoot);
            furniture.name = item.DisplayName;
            furniture.transform.localScale = Vector3.one * defaultFurnitureScale;

            // --- A MAGIA ACONTECE AQUI ---
            // 2. Extrai a inteligência espacial (PlaceableObject) do modelo que acabámos de criar
            PlaceableObject placeable = furniture.GetComponent<PlaceableObject>();
            
            if (placeable != null && selectionManager != null)
            {
                // 3. Força a Seleção e o "Snap" (Fixação). Se o objeto criado for uma janela,
                // ele saltará automaticamente para a parede mais próxima em vez de ficar a flutuar na sala.
                selectionManager.ForceSelectAndSnap(placeable);
            }
        }
    }
}