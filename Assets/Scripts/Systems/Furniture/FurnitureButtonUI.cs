using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteriorPlanner.Systems.Furniture
{
    public class FurnitureButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        private FurnitureItemData itemData;

        public void Setup(FurnitureItemData item)
        {
            itemData = item;

            if (label != null)
            {
                label.text = item.DisplayName;
            }

            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (FurnitureSelectionManager.Instance != null && itemData != null)
            {
                FurnitureSelectionManager.Instance.SelectFurniture(itemData);
            }
        }
    }
}