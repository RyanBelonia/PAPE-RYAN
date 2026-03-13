using UnityEngine;
using InteriorPlanner.Core;

namespace InteriorPlanner.Systems.Menu
{
    public class MainMenuUIController : MonoBehaviour
    {
         public void OnClickNewProject()
        {
            SceneController.LoadFloorPlanEditor();
        }

        public void OnClickOpenProject()
        {
            Debug.Log("Sistema de abrir projeto ainda não implementado.");
            SceneController.LoadFloorPlanEditor();
        }

        public void OnClickExit()
        {
            Debug.Log("A sair da aplicação...");
            Application.Quit();
        }
    }
}