using UnityEngine;
using InteriorPlanner.Core;

namespace InteriorPlanner.Systems.Menu
{
    public class MainMenuUIController : MonoBehaviour
    {
        public void OnClickNewProject()
        {
            SceneController.LoadProjectSetup();
        }

        public void OnClickOpenProject()
        {
            Debug.Log("Sistema de abrir projeto ainda não implementado.");
            SceneController.LoadProjectSetup();
        }

        public void OnClickExit()
        {
            Debug.Log("A sair da aplicação...");
            Application.Quit();
        }
    }
}