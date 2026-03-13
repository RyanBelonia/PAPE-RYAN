using UnityEngine;

namespace InteriorPlanner.Data
{
    [System.Serializable]
    public class WallSegmentData
    {
        public Vector2 StartPoint;
        public Vector2 EndPoint;

        public WallSegmentData(Vector2 startPoint, Vector2 endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}