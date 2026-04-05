using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteriorPlanner.Systems.Furniture
{
    public class FurnitureButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image thumbnailImage;

        private FurnitureItemData itemData;

        public void Setup(FurnitureItemData item)
        {
            itemData = item;

            if (nameText != null)
                nameText.text = item.DisplayName;

            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = item.Thumbnail;
                thumbnailImage.enabled = item.Thumbnail != null;
            }

            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (FurnitureSelectionManager.Instance != null)
            {
                FurnitureSelectionManager.Instance.SelectFurniture(itemData);
            }
        }
    }
}