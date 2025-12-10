using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using System.Numerics;

namespace RouteOptimizer.Helpers.RouteBuilders
{
    public static class SmoothedRouteBuilder
    {
        public static SmoothedRouteSegment BuildSmoothedRouteSegment(Point startPoint,
            Point middlePoint,
            Point endPoint,
            double middlePointZoneRadius)
        {
            var smoothPoint = GetSmoothingPoint(startPoint.Coordinates,
                                                middlePoint.Coordinates,
                                                endPoint.Coordinates,
                                                middlePointZoneRadius);

            var route = new List<RouteSegmentPoint>(capacity: 3)
            {
                new RouteSegmentPoint(startPoint, startPoint),
                new RouteSegmentPoint(middlePoint, smoothPoint),
                new RouteSegmentPoint(endPoint, endPoint)
            };

            return new SmoothedRouteSegment(route);
        }

        public static double GetRouteLength(Vector2 startPoint,
            Vector2 middlePoint,
            Vector2 endPoint,
            double middlePointZoneRadius)
        {
            // Normalize both vectors pointing to the middle point
            Vector2 v1 = Vector2.Normalize(startPoint - middlePoint);
            Vector2 v2 = Vector2.Normalize(endPoint - middlePoint);

            var smoothCenter = AngleBisectorOnCircle(v1, v2, middlePoint, middlePointZoneRadius);

            return Vector2.Distance(startPoint, smoothCenter) + Vector2.Distance(smoothCenter, endPoint);
        }

        //static Point GetSmoothingPoint(Vector2 startPoint,
        //    Vector2 middlePoint,
        //    Vector2 endPoint,
        //    double middlePointZoneRadius)
        //{
        //    // Normalize both vectors pointing to the middle point
        //    var v1 = Vector2.Normalize(middlePoint - startPoint);
        //    var v2 = Vector2.Normalize(middlePoint - endPoint);

        //    var smoothCenter = AngleBisectorOnCircle(v1, v2, middlePoint, middlePointZoneRadius);

        //    return new Point()
        //    {
        //        Type = PointType.Target,
        //        Coordinates = smoothCenter
        //    };
        //}

        //static Point GetSmoothingPoint(Vector2 startPoint,
        //                       Vector2 middlePoint,
        //                       Vector2 endPoint,
        //                       double radius)
        //{
        //    var v1 = Vector2.Normalize(startPoint - middlePoint); // FROM middle -> start
        //    var v2 = Vector2.Normalize(endPoint - middlePoint);   // FROM middle -> end

        //    var smoothCenter = AngleBisectorOnCircleRobust(v1, v2, middlePoint, radius);
        //    return new Point { Type = PointType.Target, Coordinates = smoothCenter };
        //}

        static Point GetSmoothingPoint(Vector2 start, Vector2 middle, Vector2 end, double radius)
        {
            var p = AngleBisectorOnCircleRobust_SelectBest(start, middle, end, (float)radius);
            return new Point { Type = PointType.Target, Coordinates = p };
        }

        static Vector2 AngleBisectorOnCircleRobust_SelectBest(Vector2 start, Vector2 middle, Vector2 end, float radius)
        {
            const float EPS = 1e-7f;
            // Directions from middle to neighbors
            var v1raw = start - middle;
            var v2raw = end - middle;

            // If any neighbor coincides with middle, fallback to the other direction
            bool v1Zero = v1raw.LengthSquared() < EPS;
            bool v2Zero = v2raw.LengthSquared() < EPS;
            Vector2 v1 = v1Zero ? Vector2.Zero : Vector2.Normalize(v1raw);
            Vector2 v2 = v2Zero ? Vector2.Zero : Vector2.Normalize(v2raw);

            // If both zero -> arbitrary point on circle
            if (v1Zero && v2Zero)
                return middle + new Vector2(radius, 0);

            var candidates = new List<Vector2>();

            // Candidate generator helper
            void AddCandidateDir(Vector2 dir)
            {
                if (dir.LengthSquared() < EPS) return;
                candidates.Add(Vector2.Normalize(dir));
            }

            // primary bisector(s)
            AddCandidateDir(v1 + v2);  // typical internal bisector candidate
            AddCandidateDir(v1 - v2);  // alternative candidate (useful when angle > 180)

            // perpendiculars to v1 and v2 (both directions)
            if (!v1Zero)
            {
                var perp1 = new Vector2(-v1.Y, v1.X);
                AddCandidateDir(perp1);
                AddCandidateDir(-perp1);
            }
            if (!v2Zero)
            {
                var perp2 = new Vector2(-v2.Y, v2.X);
                AddCandidateDir(perp2);
                AddCandidateDir(-perp2);
            }

            // A couple of hybrids: average of v1 with perp of v2 and vice versa
            if (!v1Zero && !v2Zero)
            {
                AddCandidateDir(v1 + Vector2.Normalize(new Vector2(-v2.Y, v2.X)));
                AddCandidateDir(v2 + Vector2.Normalize(new Vector2(-v1.Y, v1.X)));
            }

            // also include both v1 and v2 themselves (points toward neighbors)
            AddCandidateDir(v1);
            AddCandidateDir(v2);

            // Remove near-duplicates (same direction up to sign)
            var uniqueDirs = new List<Vector2>();
            foreach (var d in candidates)
            {
                if (!uniqueDirs.Any(u => Vector2.Dot(u, d) > 0.999f)) // nearly same direction
                    uniqueDirs.Add(d);
            }

            // Evaluate each candidate point on circle
            Vector2 bestPoint = middle + uniqueDirs[0] * radius; // default
            double bestScorePath = double.PositiveInfinity;
            double bestAngleScore = double.PositiveInfinity;

            foreach (var dir in uniqueDirs)
            {
                var candidate = middle + dir * radius;

                // primary metric: path length (start->p + p->end)
                double pathLen = Vector2.Distance(start, candidate) + Vector2.Distance(candidate, end);

                // secondary metric: angle deviation sum (how well dir aligns with v1 and v2)
                double angleScore = 0.0;
                if (!v1Zero) angleScore += Math.Abs(AngleBetweenRadians(dir, v1));
                if (!v2Zero) angleScore += Math.Abs(AngleBetweenRadians(dir, v2));

                // choose by pathLen first, then angleScore
                if (pathLen < bestScorePath - 1e-9 ||
                    (Math.Abs(pathLen - bestScorePath) < 1e-9 && angleScore < bestAngleScore))
                {
                    bestScorePath = pathLen;
                    bestAngleScore = angleScore;
                    bestPoint = candidate;
                }
            }

            return bestPoint;
        }

        static float AngleBetweenRadians(Vector2 a, Vector2 b)
        {
            var an = a.LengthSquared() < 1e-9f ? Vector2.Zero : Vector2.Normalize(a);
            var bn = b.LengthSquared() < 1e-9f ? Vector2.Zero : Vector2.Normalize(b);
            if (an == Vector2.Zero || bn == Vector2.Zero) return 0f;
            float d = Math.Clamp(Vector2.Dot(an, bn), -1f, 1f);
            return (float)Math.Acos(d);
        }

        static Vector2 AngleBisectorOnCircleRobust(Vector2 v1, Vector2 v2, Vector2 middle, double radius)
        {
            const float EPS = 1e-6f;

            // safe normalization checks
            if (v1 == Vector2.Zero || v2 == Vector2.Zero)
            {
                // fallback: point on circle toward end (or start)
                var fallbackDir = v1 != Vector2.Zero ? v1 : v2;
                return middle + fallbackDir * (float)radius;
            }

            // try primary bisector (v1 + v2)
            var sum = v1 + v2;
            if (sum.LengthSquared() < EPS) // nearly opposite: v1 ≈ -v2
            {
                // choose perpendicular to v1 that produces the least deviation when placed on circle
                var perp = new Vector2(-v1.Y, v1.X); // 90° rotation
                var perp2 = -perp;

                // compute scores and pick best
                var c1 = middle + Vector2.Normalize(perp) * (float)radius;
                var c2 = middle + Vector2.Normalize(perp2) * (float)radius;
                return ScorePickBetter(v1, v2, middle, c1, c2);
            }
            else
            {
                var b1 = Vector2.Normalize(sum);   // candidate bisector 1
                var b2 = -b1;                      // the opposite bisector

                var p1 = middle + b1 * (float)radius;
                var p2 = middle + b2 * (float)radius;

                return ScorePickBetter(v1, v2, middle, p1, p2);
            }
        }

        // choose between two candidate points p1/p2 by scoring how well their direction
        // matches v1 and v2 (smaller = better)
        static Vector2 ScorePickBetter(Vector2 v1, Vector2 v2, Vector2 middle, Vector2 p1, Vector2 p2)
        {
            float score1 = BisectorScore(v1, v2, middle, p1);
            float score2 = BisectorScore(v1, v2, middle, p2);
            return score1 <= score2 ? p1 : p2;
        }

        // score = |angle between bisectorDir and v1| + |angle between bisectorDir and v2|
        static float BisectorScore(Vector2 v1, Vector2 v2, Vector2 middle, Vector2 candidate)
        {
            var dir = Vector2.Normalize(candidate - middle);
            return Math.Abs(AngleBetweenRadians(dir, v1)) + Math.Abs(AngleBetweenRadians(dir, v2));
        }

        // safe angle between two normalized vectors (0..PI)
        //static float AngleBetweenRadians(Vector2 a, Vector2 b)
        //{
        //    // dot might slightly exceed [-1,1] due to FP, clamp it
        //    float d = Math.Clamp(Vector2.Dot(Vector2.Normalize(a), Vector2.Normalize(b)), -1f, 1f);
        //    return (float)Math.Acos(d);
        //}

        static Vector2 AngleBisectorOnCircle(Vector2 v1,
            Vector2 v2,
            Vector2 middle,
            double radius)
        {
            // Sum = bisector direction (normalized)
            var bisectorDir = Vector2.Normalize(v1 + v2);

            // The point on the circle along the bisector
            return middle + bisectorDir * (float)radius;
        }

        //public static float AngleBetween(Vector2 from, Vector2 to)
        //{
        //    float dot = Vector2.Dot(from, to);
        //    float magnitudeProduct = from.Length() * to.Length();

        //    // Prevent division by zero
        //    if (magnitudeProduct == 0)
        //        return 0;

        //    float cos = dot / magnitudeProduct;

        //    // Clamp to [-1, 1] to avoid NaN from rounding errors
        //    cos = Math.Clamp(cos, -1f, 1f);

        //    return MathF.Acos(cos) * (180f / MathF.PI); // result in degrees
        //}
    }
}
