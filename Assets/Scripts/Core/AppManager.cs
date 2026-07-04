using UnityEngine;

namespace InteriorPlanner.Core
{
    /// <summary>
    /// Classe central do sistema. Utiliza o padrão de arquitetura "Singleton" para garantir 
    /// que existe apenas um gestor a correr no projeto inteiro, servindo como ponte de comunicação global.
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        // Acesso estático (Singleton) que permite a qualquer outro script chamar o AppManager 
        // usando apenas AppManager.Instance, sem precisar de procurar o objeto na cena.
        public static AppManager Instance { get; private set; }

        // Guarda a sessão atual (os dados do projeto que está a ser editado neste momento)
        public ProjectSession ProjectSession { get; private set; }

        private void Awake()
        {
            // Proteção do Singleton: Se já existir um AppManager ativo e tentarmos criar outro,
            // o novo destrói-se automaticamente para não haver conflitos de memória.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Define esta instância como a oficial
            Instance = this;
            
            // Impede que a Unity destrua este objeto quando o utilizador muda de ecrã (ex: do Menu para o Planner 3D)
            DontDestroyOnLoad(gameObject);

            // Inicializa uma nova sessão em branco assim que o programa arranca
            ProjectSession = new ProjectSession();
        }
    }
}