using UnityEngine;
using InteriorPlanner.Core;
using SFB; // Biblioteca Standalone File Browser (Interface nativa do explorador do Windows)
using UnityEngine.SceneManagement; 

namespace InteriorPlanner.Systems.Menu
{
    /// <summary>
    /// Gestor lógico e fluxo de navegação do Ecrã Inicial da Aplicação.
    /// Define os caminhos de criar novos projetos e ler projetos guardados no disco (Load).
    /// </summary>
    public class MainMenuUIController : MonoBehaviour
    {
        [Header("Configurações de Cena")]
        [Tooltip("O nome exato da cena onde os móveis são colocados.")]
        [SerializeField] private string scenePlaneador3D = "Planner"; 

        /// <summary>
        /// Chamado automaticamente pelo Unity assim que o Menu Inicial abre.
        /// Garante que o rato é libertado e o sistema de cliques é reiniciado para evitar bugs de transição.
        /// </summary>
        private void Start()
        {
            // Força o rato a ficar visível e desbloqueado (caso a câmara do Planner o tenha prendido)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Reset de segurança ao EventSystem: Desliga e liga o componente para limpar 
            // referências antigas da cena 3D que pudessem estar a bloquear a UI do Menu.
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
                eventSystem.enabled = true;
            }
        }

        /// <summary>
        /// Acionado pelo botão "Novo Projeto". Prepara a RAM para começar do zero.
        /// </summary>
        public void OnClickNewProject()
        {
            // Sistema de limpeza: Garante que não há "lixo" na memória (paths antigos) a tentar forçar um load fantasma
            PlayerPrefs.DeleteKey("ProjectToLoad");
            
            // Navega para o ecrã onde o utilizador escolhe a largura/altura/profundidade da sala
            SceneController.LoadProjectSetup();
        }

        /// <summary>
        /// Acionado pelo botão "Abrir Projeto". Invoca o sistema operativo para procurar ficheiros guardados.
        /// </summary>
        public void OnClickOpenProject()
        {
            // 1. Abre a janela nativa do Windows Explorer trancada na pasta de "Saves" e filtra apenas ficheiros ".json"
            var paths = StandaloneFileBrowser.OpenFilePanel("Abrir Projeto...", Application.persistentDataPath + "/Saves/", "json", false);

            // 2. Correção de Foco: Garante que o rato volta a pertencer ao Unity após a janela do Windows fechar
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 3. Se o utilizador clicou no "X" da janela do Windows ou carregou em Cancelar, cancela o processo
            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0])) 
            {
                return;
            }

            // 4. Comunicação Inter-Cenas: Guarda o caminho exato do ficheiro no registo contínuo do Unity (PlayerPrefs).
            PlayerPrefs.SetString("ProjectToLoad", paths[0]);
            PlayerPrefs.Save(); // Força a gravação síncrona no disco para evitar falhas de leitura

            // 5. Salta o ecrã de configurar medidas e vai direto para a planta 3D!
            SceneManager.LoadScene(scenePlaneador3D);
        }

        public void OnClickExit()
        {
            Debug.Log("A sair da aplicação...");
            Application.Quit();
        }
    }
}