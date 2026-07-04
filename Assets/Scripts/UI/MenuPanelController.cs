using UnityEngine;
using UnityEngine.SceneManagement; 
using InteriorPlanner.Systems.Save; 

namespace InteriorPlanner.Systems.UI
{
    /// <summary>
    /// Controla a barra lateral (UI Panel) que contém os botões de Save, Load e Exit.
    /// Gere a animação de esconder/mostrar o painel.
    /// </summary>
    public class MenuPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform panelRect; 
        [SerializeField] private SaveManager saveManager; 

        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "MenuInicial"; 

        [Header("Positions")]
        [SerializeField] private float visibleY = 0f; 
        [SerializeField] private float hiddenY = 1200f; 

        private void Start()
        {
            HidePanel(); // Inicia com o painel escondido (UX limpa)
        }

        public void ShowPanel()
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, visibleY);
        }

        public void HidePanel()
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, hiddenY);
        }

        public void ClickSave()
        {
            if (saveManager != null)
            {
                saveManager.SaveProjectWithBrowser();
                HidePanel(); 
            }
        }

        public void ClickLoad()
        {
            if (saveManager != null)
            {
                saveManager.LoadProjectWithBrowser();
                HidePanel(); 
            }
        }

        public void ClickVoltar()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}