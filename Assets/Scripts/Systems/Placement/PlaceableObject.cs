using UnityEngine;
using System.Collections.Generic;

namespace InteriorPlanner.Systems.Placement
{
    /// <summary>
    /// O "Cérebro" individual de cada modelo 3D. Este script é anexado a todos os móveis, portas 
    /// e divisórias do projeto. Ele guarda a identidade do objeto, as suas permissões físicas 
    /// e controla a sua aparência (Filtro vermelho de erro ou realce amarelo de seleção).
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Header("Object Info")]
        [SerializeField] private PlaceableObjectType objectType = PlaceableObjectType.Furniture;

        [Header("Capabilities")]
        // Permissões que ditam como o utilizador pode interagir com este objeto
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool canRotate = true;
        [SerializeField] private bool canScale = false;
        [SerializeField] private bool requiresWallSupport = false;

        [Header("Selection Visual")]
        [SerializeField] private Renderer[] renderersToHighlight;
        [SerializeField] private Color selectedColor = Color.yellow;

        [Header("Validation Visual")]
        [SerializeField] private Material invalidMaterial; // Material vermelho semi-transparente para erros de colisão

        [Header("UI Presentation")]
        public string displayNamePT; // Nome traduzido para apresentar na UI

        // A "Chave Primária" para a Base de Dados do SaveManager. 
        // Nunca muda, mesmo que o utilizador altere a cor do móvel.
        public string originalPrefabID; 

        private bool isSelected;
        private bool isValidPosition = true;
        
        // MaterialPropertyBlock é uma técnica avançada de otimização da Unity. 
        // Permite mudar a cor do objeto enviando dados diretos para a placa gráfica (GPU),
        // em vez de criar cópias pesadas do Material na RAM do computador a cada clique.
        private MaterialPropertyBlock propertyBlock;

        // Dicionário que memoriza as texturas exatas (madeira, tecido) de cada peça do móvel
        // para conseguir restaurá-las caso o objeto fique vermelho e depois volte ao normal.
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        public PlaceableObjectType ObjectType => objectType;
        public bool CanMove => canMove;
        public bool CanRotate => canRotate;
        public bool CanScale => canScale;
        public bool RequiresWallSupport => requiresWallSupport;
        public bool IsSelected => isSelected;
        public bool IsValidPosition => isValidPosition;

        /// <summary>
        /// Injeção de dependências: Chamada pelo AutoConfigurer (no Editor) ou pelo SaveManager (no Load)
        /// para aplicar as regras lógicas à peça de forma automática.
        /// </summary>
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

            // Snapshot da Memória Visual: Assim que o móvel "nasce", grava as texturas originais de cada peça
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

        /// <summary>
        /// Ouve o sistema de físicas (ObjectMover). Se o móvel bater noutro, a posição fica inválida.
        /// </summary>
        public void SetValidState(bool isValid)
        {
            if (isValidPosition == isValid) return;
            isValidPosition = isValid;
            UpdateVisualState();
        }

        /// <summary>
        /// A "Máquina de Estados Visual". Avalia a situação do móvel e pinta-o consoante a prioridade:
        /// 1º Prioridade: Erro Físico (Fica todo Vermelho).
        /// 2º Prioridade: Selecionado (Fica com um brilho Amarelo).
        /// 3º Prioridade: Normal (Restaura as texturas de madeira e tecido).
        /// </summary>
        private void UpdateVisualState()
        {
            if (renderersToHighlight == null) return;

            foreach (var rend in renderersToHighlight)
            {
                if (rend == null) continue;

                if (!isValidPosition && invalidMaterial != null)
                {
                    // 1. ESTADO INVÁLIDO (Colisão) -> Troca tudo pelo material Vermelho de Erro
                    Material[] badMats = new Material[rend.materials.Length];
                    for (int i = 0; i < badMats.Length; i++) badMats[i] = invalidMaterial;
                    rend.materials = badMats;

                    // Limpa a cor de seleção (amarelo) da Placa Gráfica para não se misturar com o vermelho
                    rend.GetPropertyBlock(propertyBlock);
                    propertyBlock.Clear();
                    rend.SetPropertyBlock(propertyBlock);
                }
                else
                {
                    // 2. ESTADO VÁLIDO -> Restaura a lista de materiais armazenada na RAM
                    if (originalMaterials.ContainsKey(rend))
                    {
                        rend.materials = originalMaterials[rend];
                    }

                    // 3. ESTADO SELECIONADO -> Aplica a cor amarela via GPU por cima da textura do móvel
                    rend.GetPropertyBlock(propertyBlock);
                    if (isSelected)
                    {
                        // O shader precisa de ter a propriedade "_BaseColor" ou equivalente para isto brilhar
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

        /// <summary>
        /// Ponte com a ferramenta PaintTool. Quando o utilizador deita o balde de tinta numa parede,
        /// esta função garante que o "Snapshot da Memória Visual" é atualizado para a nova cor,
        /// caso contrário a parede voltaria ao branco assim que fosse deselecionada.
        /// </summary>
        public void UpdateOriginalMaterial(Material newMaterial)
        {
            if (renderersToHighlight == null) return;

            foreach (var rend in renderersToHighlight)
            {
                if (rend != null)
                {
                    // Aplica fisicamente a nova tinta
                    rend.material = newMaterial;

                    // Atualiza a memória de segurança interna
                    originalMaterials[rend] = rend.materials;
                }
            }

            // Força a atualização da máquina de estados
            UpdateVisualState();
        }
    }
}