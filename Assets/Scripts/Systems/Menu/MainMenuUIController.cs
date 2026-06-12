using UnityEngine;
using InteriorPlanner.Core;
using SFB; // A nossa nova biblioteca do Windows Explorer!
using UnityEngine.SceneManagement; // Necessário para carregar a cena do planeador diretamente

namespace InteriorPlanner.Systems.Menu
{
    public class MainMenuUIController : MonoBehaviour
    {
        [Header("Configurações de Cena")]
        [Tooltip("O nome exato da cena onde os móveis são colocados.")]
        [SerializeField] private string scenePlaneador3D = "Planner"; 

        public void OnClickNewProject()
        {
            // Garante que não há nenhum projeto antigo na memória a tentar carregar
            PlayerPrefs.DeleteKey("ProjectToLoad");
            
            // Continua com o teu fluxo normal para configurar um novo quarto
            SceneController.LoadProjectSetup();
        }

        public void OnClickOpenProject()
        {
            // 1. Abre a janela nativa do Windows Explorer
            var paths = StandaloneFileBrowser.OpenFilePanel("Abrir Projeto...", Application.persistentDataPath + "/Saves/", "json", false);

            // 2. Se o utilizador fechou a janela ou não escolheu nada, cancela a ação
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) 
            {
                return;
            }

            // 3. Guarda o caminho exato do ficheiro na memória contínua do Unity
            PlayerPrefs.SetString("ProjectToLoad", paths[0]);
            PlayerPrefs.Save();

            // 4. Salta o "ProjectSetup" e vai direto para a ação!
            // Nota: Se o teu SceneController tiver uma função para ir direto para o planeador 
            // (ex: SceneController.LoadMainScene()), podes usá-la em vez do SceneManager.
            SceneManager.LoadScene(scenePlaneador3D);
        }

        public void OnClickExit()
        {
            Debug.Log("A sair da aplicação...");
            Application.Quit();
        }
    }
}