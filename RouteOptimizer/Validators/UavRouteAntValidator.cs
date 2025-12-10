using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Exceptions;
using RouteOptimizer.Helpers;
using RouteOptimizer.Models;

namespace RouteOptimizer.Validators
{
    public class UavRouteAntValidator : IInputValidator<UavRouteAlgorithmInput>
    {
        public void Validate(UavRouteAlgorithmInput inputData)
        {
            //HasIntersectingZones(inputData.Targets, inputData.Uavs);
            HasCorrectCoverage(inputData.Uavs);
        }

        private void HasIntersectingZones(IEnumerable<Point> points, IEnumerable<Uav> uavs)
        {
            var intersectedPointsData = new List<PointZoneIntersectionData>();

            foreach (var uav in uavs)
            {
                var matrix = MatrixHelper.BuildDistanceMatrix(points.ToArray());

                var intersectedPoints = matrix.SelectMany(x => new[] { x.Point1, x.Point2 })
                                                .Distinct()
                                                .Where(p => matrix.Where(x => x.Point1.Equals(p) || x.Point2.Equals(p))
                                                                  .All(x => x.Distance <= 2 * uav.RecognitionRadius))
                                                .ToArray();

                if (!intersectedPoints.Any())
                    continue;

                intersectedPointsData.AddRange(intersectedPoints.Select(x => new PointZoneIntersectionData
                {
                    Uav = uav,
                    Points = intersectedPoints
                }));
            }

            if (intersectedPointsData.Any())
            {
                throw new PointZoneIntersectionException("Some points zones are intersected!", intersectedPointsData);
            }
        }

        private void HasCorrectCoverage(IEnumerable<Uav> uavs)
        {
            var incorrectPointsData = new List<IncorrectPointPositionData>();

            foreach (var uav in uavs)
            {
                var points = uav.ServicePoints.Concat(new Point[2] { uav.Start, uav.End }).ToArray();
                var matrix = MatrixHelper.BuildDistanceMatrix(points).ToArray();

                var incorrectUavPoints = points
                                    .Where(p => !matrix
                                        .Where(item => item.Point1 == p && item.Point2 != p)
                                        .Any(item => item.Distance <= uav.MaxDistanceRange))
                                    .ToList();

                if (!incorrectUavPoints.Any())
                    continue;

                incorrectPointsData.Add(new IncorrectPointPositionData
                {
                    Uav = uav,
                    Points = incorrectUavPoints.Distinct()
                });
            }

            if (incorrectPointsData.Any())
            {
                throw new IncorrectPointPositionException("Some points have invalid coordinates!", incorrectPointsData);
            }
        }
    }
}
