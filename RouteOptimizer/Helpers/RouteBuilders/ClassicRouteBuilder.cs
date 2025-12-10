using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using System.Numerics;

namespace RouteOptimizer.Helpers.RouteBuilders
{
    public static class ClassicRouteBuilder
    {
        public static ClassicRouteSegment BuildClassicRouteSegment(Point startPoint,
            Point middlePoint,
            Point endPoint)
        {
            var dx = endPoint.Coordinates.X - startPoint.Coordinates.X;
            var dy = endPoint.Coordinates.Y - startPoint.Coordinates.Y;

            var t = ((middlePoint.Coordinates.X - startPoint.Coordinates.X) * dx +
                    (middlePoint.Coordinates.Y - startPoint.Coordinates.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            var projX = startPoint.Coordinates.X + t * dx;
            var projY = startPoint.Coordinates.Y + t * dy;

            var projPoint = new Point
            {
                Coordinates = new Vector2(projX, projY)
            };

            var route = new List<RouteSegmentPoint>(capacity: 3)
            {
                new RouteSegmentPoint(startPoint, startPoint),
                new RouteSegmentPoint(middlePoint, projPoint),
                new RouteSegmentPoint(endPoint, endPoint)
            };

            return new ClassicRouteSegment(route);
        }

        public static ClassicRouteSegment BuildClassicRouteSegment(Point startPoint,
           Point endPoint)
        {
            var route = new List<RouteSegmentPoint>(capacity: 2)
            {
                new RouteSegmentPoint(startPoint, startPoint),
                new RouteSegmentPoint(endPoint, endPoint)
            };

            return new ClassicRouteSegment(route);
        }

        public static double GetRouteLength(Vector2 startPoint, Vector2 endPoint)
        {
            return Vector2.Distance(startPoint, endPoint);
        }

        public static double GetRouteLength(Point startPoint, Point endPoint)
        {
            return Vector2.Distance(new Vector2(startPoint.Coordinates.X, startPoint.Coordinates.Y),
                                    new Vector2(endPoint.Coordinates.X, endPoint.Coordinates.Y));
        }
    }
}
