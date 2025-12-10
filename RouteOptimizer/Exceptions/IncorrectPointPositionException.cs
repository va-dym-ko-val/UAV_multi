using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;

namespace RouteOptimizer.Exceptions
{
    public class IncorrectPointPositionException : Exception
    {
        public List<IncorrectPointPositionData> PointsData { get; set; }

        public IncorrectPointPositionException(string message, List<IncorrectPointPositionData> data)
            : base(message)
        {
            PointsData = data;
        }
    }

    public class IncorrectPointPositionData
    {
        public required Uav Uav { get; set; }
        public required IEnumerable<Point> Points { get; set; }
    }
}
