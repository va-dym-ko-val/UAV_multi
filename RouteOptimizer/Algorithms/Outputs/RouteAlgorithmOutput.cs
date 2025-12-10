using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;

namespace RouteOptimizer.Algorithms.Outputs
{
    public abstract class RouteAlgorithmOutput
    {
        public IEnumerable<Route> Routes { get; set; }
    }
}
