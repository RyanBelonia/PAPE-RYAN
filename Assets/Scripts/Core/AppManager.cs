using UnityEngine;

namespace InteriorPlanner.Core
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        public ProjectSession ProjectSession { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ProjectSession = new ProjectSession();
        }
    }
}

//teste de git