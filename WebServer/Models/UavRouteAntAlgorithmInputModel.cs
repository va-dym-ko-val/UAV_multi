using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;

namespace WebServer.Models
{
    public class UavRouteAntAlgorithmInputModel
    {
        public required PointModel[] Targets { get; set; }
        public required IEnumerable<PointUavModel> Uavs { get; set; }
    }
}
