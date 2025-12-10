using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class TargetPair
    {
        public TargetPairItem Targets { get; set; }
        public double RouteLength { get; set; }
    }

    public class TargetPairItem
    {
        public required Point Point1 { get; set; }
        public required Point Point2 { get; set; }
    }
}
