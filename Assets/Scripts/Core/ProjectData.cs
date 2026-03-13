namespace InteriorPlanner.Data
{
    [System.Serializable]
    public class ProjectData
    {
        public string ProjectName;
        public FloorPlanData FloorPlan;

        public ProjectData(FloorPlanData floorPlan)
        {
            ProjectName = "Novo Projeto";
            FloorPlan = floorPlan;
        }
    }
}