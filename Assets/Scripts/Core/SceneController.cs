using UnityEngine.SceneManagement;

namespace InteriorPlanner.Core
{
    public static class SceneController
    {
        public static void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public static void LoadFloorPlanEditor()
        {
            SceneManager.LoadScene("FloorPlanEditor");
        }

        public static void LoadPlanner()
        {
            SceneManager.LoadScene("Planner");
        }
    }
}