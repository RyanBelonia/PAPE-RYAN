using System.Collections.Generic;

namespace InteriorPlanner.Data
{
    [System.Serializable]
    public class FloorPlanData
    {
        public List<WallSegmentData> Walls = new List<WallSegmentData>();
        public float WallHeight = 2.8f;
        public float WallThickness = 0.15f;
    }
}