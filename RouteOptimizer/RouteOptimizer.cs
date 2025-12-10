using RouteOptimizer.Algorithms;
using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Algorithms.Outputs;

namespace RouteOptimizer
{
    public class RouteOptimizer<TInput, TOutput>
        where TInput : RouteAlgorithmInput
        where  TOutput: RouteAlgorithmOutput, new()
    {
        private RouteAlgorithm<TInput, TOutput> _algorithm;

        public RouteOptimizer(RouteAlgorithm<TInput, TOutput> algorithm)
        {
            _algorithm = algorithm;
        }
        
        public void SetAlgorithm(RouteAlgorithm<TInput, TOutput> algorithm)
        {
            _algorithm = algorithm;
        }

        public TOutput OptimizeRoutes(TInput input) => _algorithm.BuildRoutes(input);
    }
}
