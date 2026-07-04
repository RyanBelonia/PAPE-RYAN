using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteriorPlanner.Systems.Furniture
{
    /// <summary>
    /// Controlador lógico associado ao Prefab de cada botão no catálogo (Scroll View) da Interface.
    /// É responsável por receber os dados do móvel, atualizar a imagem/texto e ouvir o clique do rato.
    /// </summary>
    public class FurnitureButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image thumbnailImage;

        // Guarda em memória os dados do móvel que este botão representa
        private FurnitureItemData itemData;

        /// <summary>
        /// Injeta as informações visuais e lógicas no botão assim que ele é criado pelo catálogo.
        /// </summary>
        public void Setup(FurnitureItemData item)
        {
            itemData = item;

            // Preenche o nome traduzido
            if (nameText != null)
                nameText.text = item.DisplayName;

            // Carrega e ativa a miniatura se ela existir
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = item.Thumbnail;
                thumbnailImage.enabled = item.Thumbnail != null;
            }

            // Garante que o botão não dispara eventos antigos e adiciona a função OnClick a este script
            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Acionado quando o utilizador clica no botão da UI.
        /// </summary>
        private void OnClick()
        {
            // Informa o gestor global que este móvel foi escolhido para entrar no cenário
            if (FurnitureSelectionManager.Instance != null)
            {
                FurnitureSelectionManager.Instance.SelectFurniture(itemData);
            }
        }
    }
}