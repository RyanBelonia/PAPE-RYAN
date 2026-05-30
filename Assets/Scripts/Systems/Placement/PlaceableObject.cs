using UnityEngine;
using System.Collections.Generic;

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

        [Header("Validation Visual")]
        [SerializeField] private Material invalidMaterial; // NOVO: Arrasta o material vermelho para aqui!

        private bool isSelected;
        private bool isValidPosition = true;
        private MaterialPropertyBlock propertyBlock;
        
        // Guarda os materiais originais para podermos voltar a eles depois do vermelho
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public PlaceableObjectType ObjectType => objectType;
        public bool CanMove => canMove;
        public bool CanRotate => canRotate;
        public bool CanScale => canScale;
        public bool RequiresWallSupport => requiresWallSupport;
        public bool IsSelected => isSelected;
        public bool IsValidPosition => isValidPosition; 

        // A tua função original intacta!
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
            
            // Quando o jogo começa, guardamos o aspeto original de todas as almofadas e madeiras
            if (renderersToHighlight != null)
            {
                foreach (var rend in renderersToHighlight)
                {
                    if (rend != null)
                    {
                        originalMaterials[rend] = rend.materials;
                    }
                }
            }
        }

        public void Select()
        {
            if (isSelected) return;
            isSelected = true;
            UpdateVisualState();
        }

        public void Deselect()
        {
            if (!isSelected) return;
            isSelected = false;
            UpdateVisualState();
        }

        // NOVA: Função chamada pelo ObjectMover quando o objeto bate noutro
        public void SetValidState(bool isValid)
        {
            if (isValidPosition == isValid) return;
            isValidPosition = isValid;
            UpdateVisualState();
        }

        // NOVA: O "Cérebro" que decide que cor o móvel deve ter num dado momento
        private void UpdateVisualState()
        {
            if (renderersToHighlight == null) return;

            foreach (var rend in renderersToHighlight)
            {
                if (rend == null) continue;

                if (!isValidPosition && invalidMaterial != null)
                {
                    // 1. ESTADO INVÁLIDO (Bateu em algo) -> Troca tudo pelo material Vermelho
                    Material[] badMats = new Material[rend.materials.Length];
                    for (int i = 0; i < badMats.Length; i++) badMats[i] = invalidMaterial;
                    rend.materials = badMats;
                    
                    // Limpa o amarelo se estiver selecionado para não misturar cores
                    rend.GetPropertyBlock(propertyBlock);
                    propertyBlock.Clear();
                    rend.SetPropertyBlock(propertyBlock);
                }
                else
                {
                    // 2. ESTADO VÁLIDO -> Restaura os materiais originais do móvel
                    if (originalMaterials.ContainsKey(rend))
                    {
                        rend.materials = originalMaterials[rend];
                    }

                    // 3. ESTADO SELECIONADO -> Se além de válido estiver clicado, aplica o teu amarelo!
                    rend.GetPropertyBlock(propertyBlock);
                    if (isSelected)
                    {
                        propertyBlock.SetColor("_BaseColor", selectedColor);
                    }
                    else
                    {
                        propertyBlock.Clear();
                    }
                    rend.SetPropertyBlock(propertyBlock);
                }
            }
        }
        // NOVA FUNÇÃO: O Balde de Tinta chama isto para atualizar a memória do objeto!
        public void UpdateOriginalMaterial(Material newMaterial)
        {
            if (renderersToHighlight == null) return;

            foreach (var rend in renderersToHighlight)
            {
                if (rend != null)
                {
                    // Aplica fisicamente a nova tinta
                    rend.material = newMaterial;
                    
                    // Atualiza a "memória" para que ele não volte ao branco ao ser selecionado
                    originalMaterials[rend] = rend.materials; 
                }
            }
            
            // Força o objeto a re-desenhar-se com a nova textura
            UpdateVisualState(); 
        }
    }
    
}