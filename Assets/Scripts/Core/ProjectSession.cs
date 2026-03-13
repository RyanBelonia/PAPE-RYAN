using InteriorPlanner.Data;

namespace InteriorPlanner.Core
{
    public class ProjectSession
    {
        public ProjectData CurrentProject { get; private set; }

        public void CreateNewProject(FloorPlanData floorPlanData)
        {
            CurrentProject = new ProjectData(floorPlanData);
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