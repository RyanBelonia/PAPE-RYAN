using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    public enum FurnitureCategory
    {
        Bathroom,
        Beds,
        Chairs,
        Closets,
        Cushions,
        Drawers,
        Kitchen,
        Sofas,
        Tables,
        Janelas, 
        Portas
    }

   [System.Serializable]
    public class FurnitureItemData
    {
        public string DisplayName;
        public FurnitureCategory Category;
        public GameObject Prefab;
        public Sprite Thumbnail;
    }
}