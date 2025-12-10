using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;

namespace WebServer.Models
{
    public class UavRouteAntAlgorithmOutputModel
    {
        public IEnumerable<RouteOutputModel> Routes { get; set; }
    }
}
