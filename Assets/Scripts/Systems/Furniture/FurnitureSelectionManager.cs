using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    /// <summary>
    /// Classe mediadora ("Middle-man") leve. Recebe a indicação de clique da Interface Gráfica 
    /// e passa essa ordem para o FurniturePlacer materializar o móvel.
    /// </summary>
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

            // Passa a bola para o sistema de criação (Placer)
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