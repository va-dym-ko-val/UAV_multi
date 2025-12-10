using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class UavInfo
    {
        public required Uav Uav { get; set; }
        public required Route Route { get; set; }
        public required double RemainingFlightResource { get; set; }
        public required Point CurrentPoint { get; set; }
        public Point? NextPredefinedPoint { get; set; }
        public bool IsFinished { get; set; }
    }
}
