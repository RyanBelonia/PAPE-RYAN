using InteriorPlanner.Data;

namespace InteriorPlanner.Core
{
    public class ProjectSession
    {
        public ProjectData CurrentProject { get; private set; }

        public void CreateNewProject(RoomData roomData)
        {
            CurrentProject = new ProjectData(roomData);
        }

        public void ClearProject()
        {
            CurrentProject = null;
        }

        public bool HasProjectLoaded()
        {
            return CurrentProject != null;
        }
    }
}