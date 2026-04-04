using TMPro;
using UnityEngine;

namespace InteriorPlanner.UI
{
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

            if (panel != null)
            {
                Vector2 panelPos = panel.anchoredPosition;
                panelPos.x = isHidden ? hiddenPanelX : shownPanelX;
                panel.anchoredPosition = panelPos;
            }

            if (toggleButtonRect != null)
            {
                Vector2 buttonPos = toggleButtonRect.anchoredPosition;
                buttonPos.x = isHidden ? hiddenButtonX : shownButtonX;
                toggleButtonRect.anchoredPosition = buttonPos;
            }

            if (buttonText != null)
            {
                buttonText.text = isHidden ? ">" : "<";
            }
        }
    }
}