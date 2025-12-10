using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Helpers.RouteBuilders;
using RouteOptimizer.Models;

namespace RouteOptimizer.Helpers
{
    public static class MatrixHelper
    {
        public static List<DistanceMatrixItem> BuildDistanceMatrix(Point[] points)
        {
            var matrix = new List<DistanceMatrixItem>();

            var n = points.Count();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var distance = 0D;

                    if (i != j)
                        distance = ClassicRouteBuilder.GetRouteLength(points[i], points[j]);

                    matrix.Add(new DistanceMatrixItem
                    {
                        Point1 = points[i],
                        Point2 = points[j],
                        Distance = distance
                    });
                }
            }

            return matrix;
        }

        public static List<DistanceMatrixItem> BuildZeroMatrix(Point[] points)
        {
            var matrix = new List<DistanceMatrixItem>();

            var n = points.Count();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix.Add(new DistanceMatrixItem
                    {
                        Point1 = points[i],
                        Point2 = points[j],
                        Distance = 0D
                    });
                }
            }

            return matrix;
        }
    }
}
