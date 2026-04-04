using System.Collections.Generic;
using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    public class FurnitureLibraryUI : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject furnitureButtonPrefab;
        [SerializeField] private List<FurnitureItemData> furnitureItems = new();

        private void Start()
        {
            GenerateButtons();
        }

        private void GenerateButtons()
        {
            ClearButtons();

            for (int i = 0; i < furnitureItems.Count; i++)
            {
                FurnitureItemData item = furnitureItems[i];

                GameObject buttonObject = Instantiate(furnitureButtonPrefab, contentRoot);
                FurnitureButtonUI buttonUI = buttonObject.GetComponent<FurnitureButtonUI>();

                if (buttonUI != null)
                {
                    buttonUI.Setup(item);
                }
            }
        }

        private void ClearButtons()
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }
        }
    }
}