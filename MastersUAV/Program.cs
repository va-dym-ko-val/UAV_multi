using RouteOptimizer.Algorithms.AntColony;
using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Validators;
using TaskGenerator;
using TaskGenerator.Models.UavTask;

namespace MastersUAV
{
    public static class Program
    {
        private static readonly int _targetsNumber = 26;

        private static readonly CoordinatesRange _mapXRange = new CoordinatesRange()
        {
            Minimum = 0,
            Maximum = 900
        };

        private static readonly CoordinatesRange _mapYRange = new CoordinatesRange()
        {
            Minimum = 0,
            Maximum = 700
        };

        private static readonly UavConfiguration[] _uavsConfigurations = new UavConfiguration[3]
        {
            new UavConfiguration()
            {
                ServicePointsNumber = 2,
                DistanceResource = 850f,
                RecognitionRadius = 6f
            },
            new UavConfiguration()
            {
                ServicePointsNumber = 3,
                DistanceResource = 900f,
                RecognitionRadius = 4f
            },
            new UavConfiguration()
            {
                ServicePointsNumber = 2,
                DistanceResource = 1000f,
                RecognitionRadius = 5f
            }
        };

        public static void Main()
        {
            var taskGenerator = new UavTaskGenerator();

            var generatedTask = taskGenerator.GenerateTaskData(new UavTaskInputData()
            {
                TargetsNumber = _targetsNumber,
                UavsConfiguration = _uavsConfigurations,
                XCoordinates = _mapXRange,
                YCoordinates = _mapYRange
            });

            int iterations = 5000;

            var optimizer = new SmoothedAntColonyUavAlgorithm(new UavRouteAntValidator());

            for (int i = 1; i < iterations; i++)
            {
                var input = new UavRouteAntAlgorithmInput
                {
                    Iterations = i,
                    Uavs = generatedTask.Uavs,
                    Targets = generatedTask.Targets.ToArray()
                };

                var result = optimizer.BuildRoutes(input);

                var targetsNumber = result.Routes.SelectMany(x => x.Segments
                                    .Select(s => s.Points.Where(p => p.MapCoordinates.Type == RouteOptimizer.Models.PointType.Target))).Distinct().Count();

                var routesDistance = result.Routes.SelectMany(x => x.Segments.Select(s => s.Length)).Sum();

                var y = 2.5 * (targetsNumber / _targetsNumber) + 2.5 * (routesDistance / 3000);

                Console.WriteLine($"{i} {y}");

            }


            //var visualizingTargets = generatedTask.Targets.Concat(
            //    generatedTask.Uavs.Select(x => x.Start).Concat(
            //        generatedTask.Uavs.Select(x => x.End).Concat(
            //            generatedTask.Uavs.SelectMany(x => x.ServicePoints)
            //            ))).ToList();

            //TaskVisualizer.TaskVisualizer.DrawRoutes(solutions.Routes, visualizingTargets, "final_routes.png", 800, 600);

            Console.WriteLine("Optimization complete. Output saved.");
        }
    }
}