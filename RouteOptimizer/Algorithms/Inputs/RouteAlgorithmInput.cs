using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.Inputs
{
    public abstract class RouteAlgorithmInput
    {
        public required Point[] Targets { get; set; }
    }
}
