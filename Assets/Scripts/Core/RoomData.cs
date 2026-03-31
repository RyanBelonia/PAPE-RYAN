namespace InteriorPlanner.Data
{
    [System.Serializable]
    public class RoomData
    {
        public float Width;
        public float Length;
        public float Height;

        public RoomData(float width, float length, float height)
        {
            Width = width;
            Length = length;
            Height = height;
        }
    }
}