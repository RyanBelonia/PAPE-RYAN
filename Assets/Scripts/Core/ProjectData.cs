namespace InteriorPlanner.Data
{
    [System.Serializable]
    public class ProjectData
    {
        public string ProjectName;
        public RoomData Room;

        public ProjectData(RoomData room)
        {
            ProjectName = "Novo Projeto";
            Room = room;
        }
    }
}