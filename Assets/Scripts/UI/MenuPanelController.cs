using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para mudar de cena (Botão Voltar)
using InteriorPlanner.Systems.Save; // Garante acesso ao teu SaveManager

namespace InteriorPlanner.Systems.UI
{
    public class MenuPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform panelRect; // O RectTransform do teu painel de menu
        [SerializeField] private SaveManager saveManager; // Referência ao teu gestor de saves

        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "MenuInicial"; // Nome exato da tua cena de menu

        [Header("Positions")]
        [SerializeField] private float visibleY = 0f;    // Posição quando está aberto
        [SerializeField] private float hiddenY = 1200f;  // Posição quando está escondido

        private void Start()
        {
            // Garante que o painel começa escondido quando o jogo inicia
            HidePanel();
        }

        // ==========================================
        // LÓGICA DE MOVIMENTO DO PAINEL
        // ==========================================

        public void ShowPanel()
        {
            if (panelRect != null)
            {
                // Mantém o X atual e altera apenas o Y para 0
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, visibleY);
            }
        }

        public void HidePanel()
        {
            if (panelRect != null)
            {
                // Mantém o X atual e altera apenas o Y para 1200
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
            }
        }

        // ==========================================
        // AÇÕES DOS BOTÕES DE DENTRO DO PAINEL
        // ==========================================

      public void ClickSave()
        {
            if (saveManager != null)
            {
                saveManager.SaveProjectWithBrowser();
                HidePanel(); // Esconde a barra automaticamente após abrir o Windows
            }
        }

        public void ClickLoad()
        {
            if (saveManager != null)
            {
                saveManager.LoadProjectWithBrowser();
                HidePanel(); // Esconde a barra automaticamente após abrir o Windows
            }
        }

        public void ClickVoltar()
        {
            // Carrega a cena do menu principal
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}