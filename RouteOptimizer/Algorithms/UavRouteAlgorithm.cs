using RouteOptimizer.Algorithms.Inputs;

namespace RouteOptimizer.Algorithms
{
    public abstract class UavRouteAlgorithm<TInput, TOutput> : RouteAlgorithm<TInput, TOutput>
          where TInput : UavRouteAlgorithmInput
          where TOutput : UavRouteAlgorithmOutput, new()
    {
    }
}