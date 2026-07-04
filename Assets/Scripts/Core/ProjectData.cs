namespace InteriorPlanner.Data
{
    /// <summary>
    /// Classe de modelo de dados. A tag [System.Serializable] permite que a Unity converta 
    /// esta classe em JSON mais tarde para guardar o projeto no disco rígido.
    /// </summary>
    [System.Serializable]
    public class ProjectData
    {
        public string ProjectName;
        
        // Referência às medidas estruturais da divisão
        public RoomData Room;

        public ProjectData(RoomData room)
        {
            ProjectName = "Novo Projeto";
            Room = room;
        }
    }
}