using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    public class FurnitureSelectionManager : MonoBehaviour
    {
        public static FurnitureSelectionManager Instance { get; private set; }

        public FurnitureItemData SelectedFurniture { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SelectFurniture(FurnitureItemData item)
        {
            SelectedFurniture = item;

            Debug.Log("Móvel selecionado: " + item.DisplayName);

            if (FurniturePlacer.Instance != null)
            {
                FurniturePlacer.Instance.PlaceFurniture(item);
            }
            else
            {
                Debug.LogWarning("FurniturePlacer não encontrado na cena.");
            }
        }
    }
}