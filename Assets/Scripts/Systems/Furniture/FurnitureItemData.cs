using UnityEngine;

namespace InteriorPlanner.Systems.Furniture
{
    /// <summary>
    /// Define as categorias lógicas usadas para filtrar o catálogo de móveis na Interface.
    /// </summary>
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
        Portas,
        Divisorias 
    }

    /// <summary>
    /// Estrutura de dados ("Envelope") que associa o modelo 3D (Prefab) 
    /// à sua imagem 2D (Thumbnail), nome em português e categoria.
    /// </summary>
    [System.Serializable]
    public class FurnitureItemData
    {
        public string DisplayName;
        public FurnitureCategory Category;
        public GameObject Prefab;
        public Sprite Thumbnail;
    }
}