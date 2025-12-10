using System.Numerics;

namespace RouteOptimizer.Models
{
    public class Point
    {
        public Vector2 Coordinates { get; set; }
        public PointType Type { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            var point = obj as Point;
            if (point == null) return false;

            return point.Type == Type && point.Coordinates == Coordinates;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum PointType
    {
        Target,
        Start,
        Finish,
        Service
    }
}
