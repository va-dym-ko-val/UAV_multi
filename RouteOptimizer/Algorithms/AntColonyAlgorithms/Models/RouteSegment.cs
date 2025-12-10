using System.Numerics;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public abstract class RouteSegment
    {
        public List<RouteSegmentPoint> Points { get; set; } = new();

        public double Length => Points.Skip(1).Select((point, index) => 
                                            Vector2.Distance(point.DisplayCoordinates.Coordinates, Points[index].DisplayCoordinates.Coordinates)
                                        ).Aggregate((a, b) => a + b);
    }

    public class ClassicRouteSegment : RouteSegment
    {
        public ClassicRouteSegment(List<RouteSegmentPoint> points) => Points = points;
    }

    public class SmoothedRouteSegment : RouteSegment
    {
        public SmoothedRouteSegment(List<RouteSegmentPoint> points) => Points = points;
    }
}
