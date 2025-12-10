using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;

namespace RouteOptimizer.Algorithms.Inputs
{
    public class UavRouteAlgorithmInput : RouteAlgorithmInput
    {
        public required IEnumerable<Uav> Uavs { get; set; }
    }
}
