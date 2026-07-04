using UnityEngine.SceneManagement;

namespace InteriorPlanner.Core
{
    /// <summary>
    /// Classe estática responsável pela gestão e transição de cenas (Ecrãs) da aplicação.
    /// Centraliza as chamadas para evitar a digitação manual de nomes de cenas (strings) noutros scripts,
    /// prevenindo quebras no código caso o nome de uma cena seja alterado no futuro.
    /// </summary>
    public static class SceneController
    {
        public static void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public static void LoadProjectSetup()
        {
            SceneManager.LoadScene("ProjectSetup");
        }

        public static void LoadLoading()
        {
            SceneManager.LoadScene("Loading");
        }

        public static void LoadPlanner()
        {
            SceneManager.LoadScene("Planner");
        }
    }
}