using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;

namespace RouteOptimizer.Extensions
{
    public static class ListExtension
    {
        public static List<List<RouteSegmentPoint>> SplitBy(this List<RouteSegmentPoint> source, PointType delimiterType)
        {
            var result = new List<List<RouteSegmentPoint>>();
            int start = 0;

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].MapCoordinates.Type == delimiterType)
                {
                    if (i > start)
                        result.Add(source.GetRange(start, i - start));
                    start = i + 1;
                }
            }

            // Add final segment if any
            if (start < source.Count)
                result.Add(source.GetRange(start, source.Count - start));

            return result;
        }

        public static List<List<RouteSegmentPoint>> SplitIncludingDelimiter(this List<RouteSegmentPoint> source, PointType delimiterType)
        {
            var result = new List<List<RouteSegmentPoint>>();
            var current = new List<RouteSegmentPoint>();

            for (int i = 0; i < source.Count; i++)
            {
                var point = source[i];
                current.Add(point);

                if (point.MapCoordinates.Type == delimiterType)
                {
                    // finalize current segment
                    result.Add(new List<RouteSegmentPoint>(current));

                    // start new one that begins with the same delimiter
                    current = new List<RouteSegmentPoint> { point };
                }
            }

            if (current.Count > 0)
                result.Add(current);

            return result;
        }
    }
}
