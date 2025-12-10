using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;

namespace TaskGenerator.Models.UavTask
{
    public class UavTaskOutputData : TaskOutputData
    {
        public IEnumerable<Point> Targets { get; set; }
        public IEnumerable<Uav> Uavs { get; set; }

        public override string TaskName { get => "UAVTask"; set => throw new NotImplementedException(); }
    }
}
