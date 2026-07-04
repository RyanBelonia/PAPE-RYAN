using InteriorPlanner.Data;

namespace InteriorPlanner.Core
{
    /// <summary>
    /// Gestor de estado durante o tempo de execução (Runtime). 
    /// Mantém o registo de qual é o projeto atualmente aberto na memória RAM.
    /// </summary>
    public class ProjectSession
    {
        public ProjectData CurrentProject { get; private set; }

        // Inicializa uma nova sessão de trabalho injetando os dados da sala
        public void CreateNewProject(RoomData roomData)
        {
            CurrentProject = new ProjectData(roomData);
        }

        // Limpa a memória quando o utilizador fecha o projeto e volta ao menu inicial
        public void ClearProject()
        {
            CurrentProject = null;
        }

        // Verifica se existe algum projeto ativo, usado como escudo de segurança por outros scripts
        public bool HasProjectLoaded()
        {
            return CurrentProject != null;
        }
    }
}