using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class DistanceMatrixItem
    {
        public required Point Point1 { get; set; }
        public required Point Point2 { get; set; }
        public required double Distance { get; set; }
        public double Pheromone { get; set; } = 0.0001;
    }
}
