using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Algorithms.Outputs;

namespace RouteOptimizer.Algorithms
{
    public abstract class RouteAlgorithm<TInput, TOutput>
          where TInput : RouteAlgorithmInput
          where TOutput : RouteAlgorithmOutput, new()
    {
        public abstract TOutput BuildRoutes(TInput inputData);
    }
}
