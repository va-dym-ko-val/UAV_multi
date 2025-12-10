using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using WebServer.Models;

namespace WebServer
{
    public static class WebMapper
    {
        public static Point PointModelToPoint(PointModel pointModel)
        {
            return new Point
            {
                Type = pointModel.Type ?? PointType.Target,
                Coordinates = new System.Numerics.Vector2(pointModel.X, pointModel.Y)
            };
        }

        public static Uav UavModelToUav(PointUavModel pointUavModel)
        {
            return new Uav
            (
                pointUavModel.Id,
                PointModelToPoint(pointUavModel.Start),
                PointModelToPoint(pointUavModel.End),
                pointUavModel.ServicePoints.Select(PointModelToPoint).ToList(),
                pointUavModel.MaxDistanceRange,
                pointUavModel.RecognitionRadius
            );
        }

        public static PointModel PointToPointModel(Point point)
        {
            return new PointModel
            {
                X = point.Coordinates.X,
                Y = point.Coordinates.Y,
                Type = point.Type
            };
        }

        public static PointUavModel UavToUavModel(Uav uav)
        {
            return new PointUavModel
            {
                Id = uav.Id,
                Start = PointToPointModel(uav.Start),
                End = PointToPointModel(uav.End),
                ServicePoints = uav.ServicePoints.Select(PointToPointModel).ToList(),
                MaxDistanceRange = uav.MaxDistanceRange,
                RecognitionRadius = uav.RecognitionRadius
            };
        }

        public static RouteSegmentModel RouteSegmentToRouteSegmentModel(RouteSegment routeSegment)
        {
            return new RouteSegmentModel
            {
                Length = routeSegment.Length,
                Points = routeSegment.Points.Select(x => new RouteSegmentPointOutputModel
                {
                    MapCoordinates = PointToPointModel(x.MapCoordinates),
                    DisplayCoordinates = PointToPointModel(x.DisplayCoordinates),
                    Type = x.Type
                }).ToList(),
                SegmentType = routeSegment is ClassicRouteSegment ? RouteSegmentType.Classic : RouteSegmentType.Smoothed
            };
        }
    }
}
