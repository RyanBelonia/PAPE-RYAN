using UnityEngine;

namespace InteriorPlanner.Systems.Placement
{
    public class PlaceableObject : MonoBehaviour
    {
        [Header("Object Info")]
        [SerializeField] private PlaceableObjectType objectType = PlaceableObjectType.Furniture;

        [Header("Capabilities")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool canRotate = true;
        [SerializeField] private bool canScale = false;
        [SerializeField] private bool requiresWallSupport = false;

        [Header("Selection Visual")]
        [SerializeField] private Renderer[] renderersToHighlight;
        [SerializeField] private Color selectedColor = Color.yellow;

        private bool isSelected;
        private MaterialPropertyBlock propertyBlock;

        public PlaceableObjectType ObjectType => objectType;
        public bool CanMove => canMove;
        public bool CanRotate => canRotate;
        public bool CanScale => canScale;
        public bool RequiresWallSupport => requiresWallSupport;
        public bool IsSelected => isSelected;

        public void Configure(
            PlaceableObjectType type,
            bool move,
            bool rotate,
            bool scale,
            bool wallSupport,
            Renderer[] renderers)
        {
            objectType = type;
            canMove = move;
            canRotate = rotate;
            canScale = scale;
            requiresWallSupport = wallSupport;
            renderersToHighlight = renderers;
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        public void Select()
        {
            if (isSelected) return;

            isSelected = true;
            ApplySelectionVisual(true);
        }

        public void Deselect()
        {
            if (!isSelected) return;

            isSelected = false;
            ApplySelectionVisual(false);
        }

        private void ApplySelectionVisual(bool selected)
        {
            if (renderersToHighlight == null || renderersToHighlight.Length == 0) return;

            for (int i = 0; i < renderersToHighlight.Length; i++)
            {
                Renderer rend = renderersToHighlight[i];
                if (rend == null) continue;

                rend.GetPropertyBlock(propertyBlock);

                if (selected)
                    propertyBlock.SetColor("_BaseColor", selectedColor);
                else
                    propertyBlock.Clear();

                rend.SetPropertyBlock(propertyBlock);
            }
        }
    }
}