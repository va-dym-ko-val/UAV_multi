using System.Numerics;

namespace RouteOptimizer.Algorithms.AntColonyAlgorithms.Models
{
    public class Route
    {
        public Uav Uav { get; private set; }
        public List<RouteSegment> Segments { get; set; } = new();

        public Route(Uav uav)
        {
            Uav = uav;
        }

        public void AddRouteSegment(RouteSegment segment) => Segments.Add(segment);

        public double CalculateDistance()
        {
            var points = GetAllPoints();

            return points.Skip(1).Select((segmentPoint, index) =>
                                            Vector2.Distance(segmentPoint.DisplayCoordinates.Coordinates, points[index].DisplayCoordinates.Coordinates)
                                        ).Aggregate((a, b) => a + b);
        }

        public List<RouteSegmentPoint> GetAllPoints() => Segments.SelectMany(x => x.Points).ToList();
    }
}
