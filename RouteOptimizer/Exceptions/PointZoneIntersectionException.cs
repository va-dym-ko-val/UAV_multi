using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;

namespace RouteOptimizer.Exceptions
{
    public class PointZoneIntersectionException : Exception
    {
        public List<PointZoneIntersectionData> PointsData { get; set; }

        public PointZoneIntersectionException(string message, List<PointZoneIntersectionData> data)
            : base(message)
        {
            PointsData = data;
        }
    }

    public class PointZoneIntersectionData
    {
        public required Uav Uav { get; set; }
        public required IEnumerable<Point> Points { get; set; }
    }
}
