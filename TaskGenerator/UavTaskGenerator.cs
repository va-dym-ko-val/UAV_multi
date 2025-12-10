using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using System.Numerics;
using TaskGenerator.Models.UavTask;
using Point = RouteOptimizer.Models.Point;

namespace TaskGenerator
{
    public class UavTaskGenerator : ITaskGenerator<UavTaskInputData, UavTaskOutputData>
    {
        public UavTaskOutputData GenerateTaskData(UavTaskInputData input)
        {
            var targets = GenerateRandomPoints(input.TargetsNumber, input.XCoordinates, input.YCoordinates);

            var uavs = GenerateRandomUavs(input.UavsConfiguration, input.XCoordinates, input.YCoordinates);

            return new UavTaskOutputData()
            {
                Targets = targets,
                Uavs = uavs
            };
        }

        private IEnumerable<Point> GenerateRandomPoints(int count,
            CoordinatesRange xRange,
            CoordinatesRange yRange,
            PointType type = PointType.Target)
        {
            var points = new List<Point>(capacity: count);

            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                var x = rand.Next(xRange.Minimum, xRange.Maximum);
                var y = rand.Next(yRange.Minimum, yRange.Maximum);

                if (!points.Exists(p => p.Coordinates.X == x && p.Coordinates.Y == y))
                {
                    points.Add(new Point()
                    {
                        Coordinates = new Vector2(x, y),
                        Type = type
                    });
                }

                Console.WriteLine($"Point {i}: ({x}, {y})");
            }

            return points;
        }

        private IEnumerable<Uav> GenerateRandomUavs(UavConfiguration[] configurations,
            CoordinatesRange xRange,
            CoordinatesRange yRange)
        {
            var uavs = new List<Uav>(capacity: configurations.Count());

            for (int i = 0; i < configurations.Count(); i++)
            {
                var start = GenerateRandomPoints(1,
                   xRange, yRange,
                   PointType.Start)
                   .First();

                var end = GenerateRandomPoints(1,
                    xRange, yRange,
                    PointType.Finish)
                    .First();

                var servicePoints = GenerateRandomPoints(configurations[i].ServicePointsNumber,
                    xRange, yRange,
                    PointType.Service);

                uavs.Add(new Uav(i, start, end,
                    servicePoints.ToList(),
                    configurations[i].DistanceResource,
                    configurations[i].RecognitionRadius));
            }

            return uavs;
        }
    }
}