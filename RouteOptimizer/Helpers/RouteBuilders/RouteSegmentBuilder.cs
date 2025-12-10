using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using System.Net;
using System.Numerics;

namespace RouteOptimizer.Helpers.RouteBuilders
{
    public static class RouteSegmentBuilder
    {
        public static RouteSegment BuildRouteSegment(Point startPoint,
           Point middlePoint,
           Point endPoint,
           double middlePointZoneRadius,
           IEnumerable<Point> allAvailablePoints)
        {
            // Case: sometimes we can just fly from the 1st to the 3rd point as a straight line and be
            //       able to film the middle (by camera radius and coordinates)
            var classicRoute = TryBuildClassicRouteSegment(startPoint, middlePoint, endPoint, middlePointZoneRadius);

            if (classicRoute != null)
            {
                // Case: can be that even more intermediate can be filmed by camera using the direct route
                //       that means such route includes more targets at a time than a smoothed despite the length
                var classicEnhancedRoute = TryBuildClassicEnhancedRouteSegment(startPoint, endPoint, middlePointZoneRadius, allAvailablePoints);

                if (classicEnhancedRoute != null)
                {
                    return classicEnhancedRoute;
                }

                // Case: since both classic and smoothed routes includes the same number (3) of points
                //       we need to compare their length and return more effective (with less distacne)
                var smoothedRoute = TryBuildSmoothedEnhancedRouteSegment(startPoint, middlePoint, endPoint, middlePointZoneRadius, allAvailablePoints);

                return smoothedRoute.Length <= classicRoute.Length ? smoothedRoute : classicRoute;
            }

            // Case: if classic route with the middle point is not valid, then smoothed route must be used
            return SmoothedRouteBuilder.BuildSmoothedRouteSegment(startPoint, middlePoint, endPoint, middlePointZoneRadius);
        }

        static ClassicRouteSegment TryBuildClassicRouteSegment(Point startPoint,
            Point middlePoint,
            Point endPoint,
            double middlePointZoneRadius)
        {
            var classicRoute = ClassicRouteBuilder.BuildClassicRouteSegment(startPoint, middlePoint, endPoint);

            var fromMiddlePointToSegmentLength = ClassicRouteBuilder.GetRouteLength(middlePoint, classicRoute.Points[1].DisplayCoordinates);

            if (fromMiddlePointToSegmentLength <= middlePointZoneRadius)
            {
                return classicRoute;
            }

            return null;
        }

        static ClassicRouteSegment TryBuildClassicEnhancedRouteSegment(Point startPoint,
            Point endPoint,
            double middlePointZoneRadius,
            IEnumerable<Point> allAvailablePoints)
        {
            if (allAvailablePoints == null || !allAvailablePoints.Any())
                return null;

            var visiblePoints = GetVisiblePointDuringTheRoute(allAvailablePoints.Except(new Point[2] { startPoint, endPoint }).ToList(),
                startPoint,
                endPoint,
                middlePointZoneRadius);

            if (visiblePoints.Count > 1)
            {
                visiblePoints = visiblePoints.OrderBy(x => x.t).ToList();

                var length = 2 + visiblePoints.Count;
                var route = Enumerable.Repeat(new RouteSegmentPoint(new Point(), new Point()), length).ToList();

                for (var i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        route[i] = new RouteSegmentPoint(startPoint, startPoint);
                    }
                    else if (i == length - 1)
                    {
                        route[i] = new RouteSegmentPoint(endPoint, endPoint);
                    }
                    else
                    {
                        route[i] = new RouteSegmentPoint(visiblePoints[i - 1].point, visiblePoints[i - 1].projPoint);
                    }
                }

                return new ClassicRouteSegment(route);
            }

            return null;
        }

        static SmoothedRouteSegment TryBuildSmoothedEnhancedRouteSegment(Point startPoint,
            Point middlePoint,
            Point endPoint,
            double middlePointZoneRadius,
            IEnumerable<Point> allAvailablePoints)
        {
            var segment = SmoothedRouteBuilder.BuildSmoothedRouteSegment(startPoint, middlePoint, endPoint, middlePointZoneRadius);

            var visiblePointsTo = GetVisiblePointDuringTheRoute(allAvailablePoints.Except(new Point[2] { startPoint, middlePoint }).ToList(),
                startPoint,
                segment.Points[1].DisplayCoordinates,
                middlePointZoneRadius);

            var toRoute = new List<RouteSegmentPoint> { segment.Points[0], segment.Points[1] };
            if (visiblePointsTo.Any())
            {
                visiblePointsTo = visiblePointsTo.OrderBy(x => x.t).ToList();

                var length = 2 + visiblePointsTo.Count;
                toRoute = Enumerable.Repeat(new RouteSegmentPoint(new Point(), new Point()), length).ToList();

                for (var i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        toRoute[i] = new RouteSegmentPoint(startPoint, startPoint);
                    }
                    else if (i == length - 1)
                    {
                        toRoute[i] = new RouteSegmentPoint(segment.Points[1].MapCoordinates, segment.Points[1].DisplayCoordinates);
                    }
                    else
                    {
                        toRoute[i] = new RouteSegmentPoint(visiblePointsTo[i - 1].point, visiblePointsTo[i - 1].projPoint);
                    }
                }
            }

            var visiblePointsFrom = GetVisiblePointDuringTheRoute(allAvailablePoints.Except(new Point[2] { middlePoint, endPoint }).ToList(),
                segment.Points[1].DisplayCoordinates,
                endPoint,
                middlePointZoneRadius);

            var fromRoute = new List<RouteSegmentPoint> { segment.Points[1], segment.Points[2] };
            if (visiblePointsFrom.Any())
            {
                visiblePointsFrom = visiblePointsFrom.OrderBy(x => x.t).ToList();

                var length = 2 + visiblePointsFrom.Count;
                fromRoute = Enumerable.Repeat(new RouteSegmentPoint(new Point(), new Point()), length).ToList();

                for (var i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        fromRoute[i] = new RouteSegmentPoint(segment.Points[1].MapCoordinates, segment.Points[1].DisplayCoordinates);
                    }
                    else if (i == length - 1)
                    {
                        fromRoute[i] = new RouteSegmentPoint(endPoint, endPoint);
                    }
                    else
                    {
                        fromRoute[i] = new RouteSegmentPoint(visiblePointsFrom[i - 1].point, visiblePointsFrom[i - 1].projPoint);
                    }
                }
            }

            fromRoute.RemoveAt(0);

            var fullRoute = toRoute.Concat(fromRoute).ToList();
            return new SmoothedRouteSegment(fullRoute);
        }

        static List<(Point point, Point projPoint, double t)> GetVisiblePointDuringTheRoute(List<Point> allAvailablePoints,
            Point startPoint,
            Point endPoint,
            double middlePointZoneRadius)
        {
            var dx = endPoint.Coordinates.X - startPoint.Coordinates.X;
            var dy = endPoint.Coordinates.Y - startPoint.Coordinates.Y;
            var segmentLengthSquared = dx * dx + dy * dy;

            var visiblePoints = new List<(Point point, Point projPoint, double t)>();

            foreach (var middle in allAvailablePoints.Except(new Point[2] { startPoint, endPoint }))
            {
                // Compute projection factor t (clamped to segment)
                var t = ((middle.Coordinates.X - startPoint.Coordinates.X) * dx +
                         (middle.Coordinates.Y - startPoint.Coordinates.Y) * dy) / segmentLengthSquared;
                t = Math.Clamp(t, 0, 1);

                // Projection point on the segment
                var projX = startPoint.Coordinates.X + t * dx;
                var projY = startPoint.Coordinates.Y + t * dy;

                var projPoint = new Point
                {
                    Coordinates = new Vector2(projX, projY)
                };

                // Distance from middle point to its projection on segment
                var fromMiddlePointToSegmentLength = Vector2.Distance(middle.Coordinates, projPoint.Coordinates);

                // If within recognition zone → visible
                if (fromMiddlePointToSegmentLength <= middlePointZoneRadius)
                {
                    visiblePoints.Add((middle, projPoint, t));
                }
            }

            return visiblePoints;
        }
    }
}
