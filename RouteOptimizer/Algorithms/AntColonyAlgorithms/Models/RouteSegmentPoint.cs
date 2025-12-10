using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class RouteSegmentPoint
    {
        public Point MapCoordinates { get; set; }
        public Point DisplayCoordinates { get; set; }
        public PointType Type { get; set; }

        public RouteSegmentPoint(Point mapCoordinates, Point displayCoordinates)
        {
            MapCoordinates = mapCoordinates;
            DisplayCoordinates = displayCoordinates;
            Type = mapCoordinates.Type;
        }
    }
}
