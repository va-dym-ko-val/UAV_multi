using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class Uav
    {
        public int Id { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }
        public List<Point> ServicePoints { get; set; }
        public double MaxDistanceRange { get; set; }
        public double RecognitionRadius { get; set; }

        public Uav(int id, Point start, Point end, List<Point> services, double maxRange, double recognitionRadius)
        {
            Id = id;
            Start = start;
            End = end;
            ServicePoints = services;
            MaxDistanceRange = maxRange;
            RecognitionRadius = recognitionRadius;
        }
    }
}
