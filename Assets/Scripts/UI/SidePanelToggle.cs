using UnityEngine;
using TMPro;

namespace InteriorPlanner.UI
{
    /// <summary>
    /// Pequena ferramenta de UX que permite ao utilizador expandir ou recolher 
    /// o painel lateral do catálogo para ganhar mais espaço de visualização na sala.
    /// </summary>
    public class SidePanelToggle : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform toggleButtonRect;
        [SerializeField] private TMP_Text buttonText;

        [Header("Panel Positions")]
        [SerializeField] private float shownPanelX = 0f;
        [SerializeField] private float hiddenPanelX = -280f;

        [Header("Button Positions")]
        [SerializeField] private float shownButtonX = -1791f;
        [SerializeField] private float hiddenButtonX = -2110f;

        private bool isHidden = false;

        public void TogglePanel()
        {
            isHidden = !isHidden;

            // Alterna a posição do painel entre mostrado e escondido
            if (panel != null)
            {
                Vector2 panelPos = panel.anchoredPosition;
                panelPos.x = isHidden ? hiddenPanelX : shownPanelX;
                panel.anchoredPosition = panelPos;
            }

            // Move o botão em conjunto para que ele fique sempre colado à borda do painel
            if (toggleButtonRect != null)
            {
                Vector2 buttonPos = toggleButtonRect.anchoredPosition;
                buttonPos.x = isHidden ? hiddenButtonX : shownButtonX;
                toggleButtonRect.anchoredPosition = buttonPos;
            }

            // Alterna o símbolo gráfico no botão ('>' para abrir, '<' para fechar)
            if (buttonText != null)
            {
                buttonText.text = isHidden ? ">" : "<";
            }
        }
    }
}