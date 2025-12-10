namespace RouteOptimizer.Algorithms.Inputs
{
    public class UavRouteAntAlgorithmInput : UavRouteAlgorithmInput
    {
        public int Iterations { get; set; } = 10000;
        public double Alpha { get; set; } = 0.87;
        public double Beta { get; set; } = 2.08;
        public double Evaporation { get; set; } = 0.36;
    }
}
