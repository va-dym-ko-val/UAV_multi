using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Helpers;
using RouteOptimizer.Helpers.RouteBuilders;
using RouteOptimizer.Models;
using RouteOptimizer.Validators;
using System.Linq;

namespace RouteOptimizer.Algorithms.AntColony
{
    public class SmoothedAntColonyUavAlgorithm : UavRouteAlgorithm<UavRouteAntAlgorithmInput, UavRouteAlgorithmOutput>
    {
        private readonly Random _rand = new();
        private readonly IInputValidator<UavRouteAlgorithmInput> _validator;

        private List<DistanceMatrixItem> _pheromonesMatrix = new();

        public SmoothedAntColonyUavAlgorithm(IInputValidator<UavRouteAlgorithmInput> validator)
        {
            _validator = validator;
        }

        public override UavRouteAlgorithmOutput BuildRoutes(UavRouteAntAlgorithmInput inputData)
        {
            PrepareInputData(inputData);

            _validator.Validate(inputData);

            var bestSolution = default(List<Route>);
            var bestRouteDistance = double.MaxValue;
            var bestTargetsCovered = int.MinValue;

            for (var iteration = 0; iteration < inputData.Iterations; iteration++)
            {
                var availableTargets = new List<Point>(inputData.Targets);

                var solution = new List<Route>();

                var uavOrder = inputData.Uavs
                    .OrderBy(_ => _rand.Next())
                    .Select(x => new UavInfo
                    {
                        Uav = x,
                        Route = new Route(x),
                        RemainingFlightResource = x.MaxDistanceRange,
                        CurrentPoint = x.Start
                    }).ToList();

                var currentUav = uavOrder[0];
                var lastUav = uavOrder[uavOrder.Count - 1];

                // until all of them reached end or all targets are examined
                while (!uavOrder.TrueForAll(x => x.IsFinished))
                {
                    // Case: no targets are left; active UAV must end the route at finish point,
                    //       sometimes using service points (A2.1)
                    if (!availableTargets.Any())
                    {
                        var (length, segment) = GetEndPointReachableShortestSegment(currentUav);

                        if (segment == null)
                        {
                            throw new Exception("To finish the route is impossible! Algorithm error");
                        }

                        // TODO: maybe we can smooth way from the last pair of target to the end (or first service point of the created chain)
                        // if the last segment was Smoothed!
                        /////////////////////////////
                        ///
                        // TODO: add a segment
                        currentUav.Route.AddRouteSegment(segment);
                    }
                    // Case: one target is left; We analyze segment with the third point as the finish point for the UAV
                    else if (availableTargets.Count == 1)
                    {
                        if (currentUav.NextPredefinedPoint != null)
                        {
                            var tempLastPoint = new RouteSegmentPoint(currentUav.NextPredefinedPoint, currentUav.NextPredefinedPoint);
                            currentUav.Route.Segments[^1].Points.Add(tempLastPoint);

                            var (length2, segment2) = GetEndPointReachableShortestSegment(currentUav);

                            if (segment2 == null)
                            {
                                throw new Exception("To finish the route is impossible! Algorithm error");
                            }

                            currentUav.Route.Segments[^1].Points.Remove(tempLastPoint);

                            var routeSegment = RouteSegmentBuilder.BuildRouteSegment(currentUav.CurrentPoint, currentUav.NextPredefinedPoint,
                                                                                     segment2.Points[0].DisplayCoordinates, currentUav.Uav.RecognitionRadius,
                                                                                     availableTargets);

                            currentUav.Route.AddRouteSegment(routeSegment);
                            currentUav.Route.AddRouteSegment(segment2);
                        }
                        else
                        {
                            var (length3, segment3) = GetEndPointReachableShortestSegment(currentUav);

                            if (segment3 == null)
                            {
                                throw new Exception("To finish the route is impossible! Algorithm error");
                            }

                            // TODO: maybe we can smooth way from the last pair of target to the end (or first service point of the created chain)
                            // if the last segment was Smoothed!
                            /////////////////////////////
                            ///
                            // TODO: add a segment
                            currentUav.Route.AddRouteSegment(segment3);
                        }
                    }
                    else
                    {
                        var availableTargetsPairs = availableTargets
                                                    .SelectMany((x, i) => availableTargets
                                                        .Skip(i + 1)
                                                        .Select(y => new TargetPairItem
                                                        {
                                                            Point1 = x,
                                                            Point2 = y
                                                        }))
                                                    .ToList();

                        // to exclude nextPredefinedPoint(s) of next UAVs except current one
                        availableTargetsPairs = availableTargetsPairs
                            .Where(x => !uavOrder.Except(new[] { currentUav }).Any(u => u.NextPredefinedPoint == x.Point1 || u.NextPredefinedPoint == x.Point2))
                            .ToList();

                        if (currentUav.NextPredefinedPoint != null)
                        {
                            foreach (var pair in availableTargetsPairs)
                            {
                                if (pair.Point1 == currentUav.NextPredefinedPoint)
                                    continue;

                                var tempTarget = pair.Point1;
                                pair.Point1 = pair.Point2;
                                pair.Point2 = tempTarget;
                            }

                            availableTargetsPairs = availableTargetsPairs.Where(x => x.Point1 == currentUav.NextPredefinedPoint).ToList();
                        }

                        // Candidates-targets which we have enough resource to fly to
                        // (and then to the end with or without active service point)
                        var targetsPairsCandidates = availableTargetsPairs
                                    .Select(t => new TargetPair
                                    {
                                        Targets = new TargetPairItem { Point1 = t.Point1, Point2 = t.Point2 },
                                        RouteLength = SmoothedRouteBuilder.GetRouteLength(currentUav.CurrentPoint.Coordinates,
                                                                                              t.Point1.Coordinates,
                                                                                              t.Point2.Coordinates,
                                                                                              currentUav.Uav.RecognitionRadius)
                                    })
                                    .Where(t => t.RouteLength <= currentUav.RemainingFlightResource &&
                                                CanReachEndPoint(t, currentUav)) // TODO: or classic route is allowed and distance is OK?
                                    .ToList();

                        // Case: no targets candidates; need to find service candidates
                        if (!targetsPairsCandidates.Any())
                        {
                            // Candidates-services which we have enough resource to flight to (and then after updating resource to max flight to the end)
                            var serviceCandidates = currentUav.Uav.ServicePoints
                                 .Select(x => new
                                 {
                                     ServiceCandidate = x,
                                     TargetPair = new TargetPair
                                     {
                                         Targets = new TargetPairItem { Point1 = x, Point2 = currentUav.Uav.End },
                                         RouteLength = ClassicRouteBuilder.GetRouteLength(x, currentUav.Uav.End)
                                     }
                                 })
                                 .Where(s => s.TargetPair.RouteLength <= currentUav.RemainingFlightResource &&
                                             CanReachEndPoint(s.TargetPair, currentUav))
                                 .ToList();

                            // Case: no service candidates; need to to finish the route (fly to the end)
                            if (!serviceCandidates.Any())
                            {
                                // No candidates for targets and services -> the route for the UAV is ended
                                RouteSegment routeSegment = currentUav.NextPredefinedPoint != null ?
                                               SmoothedRouteBuilder.BuildSmoothedRouteSegment(currentUav.CurrentPoint, currentUav.NextPredefinedPoint,
                                                                                              currentUav.Uav.End, currentUav.Uav.RecognitionRadius) :
                                               ClassicRouteBuilder.BuildClassicRouteSegment(currentUav.CurrentPoint, currentUav.Uav.End);

                                currentUav.Route.AddRouteSegment(routeSegment);
                            }
                            // Case: service candidates found; use random of them
                            else
                            {
                                var service = serviceCandidates[_rand.Next(serviceCandidates.Count)];

                                if (currentUav.Route.Segments.Any())
                                {
                                    var lastSegmentPoints = currentUav.Route.Segments[currentUav.Route.Segments.Count - 1].Points;

                                    // sometimes in this case for smoothed type some points can be covered during the route
                                    var pointsAdd = GetVisiblePoints(availableTargets, lastSegmentPoints.Last().DisplayCoordinates, 
                                                                    service.ServiceCandidate, currentUav.Uav.RecognitionRadius);

                                    if (pointsAdd.Any())
                                    {
                                        foreach (var po in pointsAdd)
                                        {
                                            var t = GetProjectionT(po, lastSegmentPoints.Last().DisplayCoordinates, service.ServiceCandidate);
                                            var x = lastSegmentPoints.Last().DisplayCoordinates.Coordinates.X + 
                                                t * (service.ServiceCandidate.Coordinates.X - lastSegmentPoints.Last().DisplayCoordinates.Coordinates.X);
                                            var y = lastSegmentPoints.Last().DisplayCoordinates.Coordinates.Y + 
                                                t * (service.ServiceCandidate.Coordinates.Y - lastSegmentPoints.Last().DisplayCoordinates.Coordinates.Y);

                                            var projection = new Point
                                            {
                                                Coordinates = new System.Numerics.Vector2((float)x, (float)y),
                                                Type = PointType.Target
                                            };

                                            lastSegmentPoints.Add(new RouteSegmentPoint(po, projection));
                                            availableTargets.Remove(po);
                                        }
                                    }

                                    lastSegmentPoints.Add(new RouteSegmentPoint(service.ServiceCandidate, service.ServiceCandidate));
                                }
                                else
                                {
                                    var routeSegment = ClassicRouteBuilder.BuildClassicRouteSegment(currentUav.CurrentPoint, service.ServiceCandidate);
                                    currentUav.Route.AddRouteSegment(routeSegment);
                                }

                                currentUav.RemainingFlightResource = currentUav.Uav.MaxDistanceRange;
                                currentUav.CurrentPoint = service.ServiceCandidate;

                                currentUav.Uav.ServicePoints.Remove(service.ServiceCandidate);
                            }
                        }
                        // Case: targets pair candidates found; use random of them
                        else
                        {
                            //var targetsPair = targetsPairsCandidates[_rand.Next(targetsPairsCandidates.Count)];

                            var pairProbabilities = CalculateTransitionProbabilities(targetsPairsCandidates,
                                                                               inputData.Alpha,
                                                                               inputData.Beta);

                            // cumulative selection
                            double r = _rand.NextDouble();
                            double cumulative = 0;
                            KeyValuePair<(Point, Point), double>? selectedKvp = null;
                            foreach (var kvp in pairProbabilities)
                            {
                                cumulative += kvp.Value;
                                if (r <= cumulative)
                                {
                                    selectedKvp = kvp;
                                    break;
                                }
                            }
                            if (!selectedKvp.HasValue)
                                selectedKvp = pairProbabilities.First(); // fallback

                            // знайдемо відповідний TargetPair в targetsPairsCandidates
                            var chosenPair = targetsPairsCandidates.First(tp =>
                                (tp.Targets.Point1 == selectedKvp.Value.Key.Item1 && tp.Targets.Point2 == selectedKvp.Value.Key.Item2) ||
                                (tp.Targets.Point2 == selectedKvp.Value.Key.Item1 && tp.Targets.Point1 == selectedKvp.Value.Key.Item2));

                            var routeSegment = RouteSegmentBuilder.BuildRouteSegment(currentUav.CurrentPoint, chosenPair.Targets.Point1,
                                                                                     chosenPair.Targets.Point2, currentUav.Uav.RecognitionRadius,
                                                                                     availableTargets);

                            if (routeSegment is SmoothedRouteSegment)
                            {
                                currentUav.CurrentPoint = routeSegment.Points[1].DisplayCoordinates;
                                currentUav.NextPredefinedPoint = routeSegment.Points[^1].DisplayCoordinates;

                                routeSegment.Points.RemoveAt(routeSegment.Points.Count - 1);

                                availableTargets.Remove(currentUav.CurrentPoint);
                                availableTargets.Remove(chosenPair.Targets.Point1);
                            }
                            else
                            {
                                currentUav.CurrentPoint = routeSegment.Points[routeSegment.Points.Count - 1].DisplayCoordinates;
                                currentUav.NextPredefinedPoint = null;

                                availableTargets.RemoveAll(p => routeSegment.Points.Select(x => x.MapCoordinates).Any(x => x == p));
                            }

                            currentUav.Route.AddRouteSegment(routeSegment);
                            currentUav.RemainingFlightResource -= routeSegment.Length;
                        }
                    }

                    currentUav.IsFinished = currentUav.Route.GetAllPoints().Select(x => x.MapCoordinates).Contains(currentUav.Uav.End);

                    currentUav = ChooseNextUav(currentUav, lastUav, uavOrder);
                }

                EnhanceUAVFinishSegment(uavOrder, availableTargets);

                var routes = uavOrder.Select(x => x.Route).ToList();
                solution.AddRange(routes);

                var totalCovered = solution.Sum(r => r.Segments.SelectMany(x => x.Points).Count(x => x.DisplayCoordinates.Type != PointType.Service));
                var totalDistance = solution.Sum(r => r.CalculateDistance());

                if (totalCovered >= bestTargetsCovered && totalDistance <= bestRouteDistance)
                {
                    bestRouteDistance = totalDistance;
                    bestTargetsCovered = totalCovered;

                    bestSolution = solution;
                }

                UpdatePheromones(routes, routes.Select(x => x.GetAllPoints().Count).ToList(), inputData.Targets.Length, inputData.Evaporation);
            }

            //foreach (var routeSolution in bestSolution!)
            //{
            //    var newSolutionSegments = new List<RouteSegment>();

            //    for (var i = 0; i < routeSolution.Segments.Count; i++)
            //    {
            //        var segmentSegments = new List<RouteSegment>();

            //        if (!routeSolution.Segments[i].Points.Any(x => x.MapCoordinates.Type == PointType.Service))
            //        {
            //            segmentSegments.Add(routeSolution.Segments[i]);
            //        }
            //        else
            //        {
            //            var splitSegments = routeSolution.Segments[i].Points.SplitIncludingDelimiter(PointType.Service);

            //            foreach (var sgm in splitSegments)
            //            {
            //                if (sgm.Count > 2)
            //                {
            //                    segmentSegments.Add(new SmoothedRouteSegment(sgm));
            //                }
            //                else
            //                {
            //                    segmentSegments.Add(new ClassicRouteSegment(sgm));
            //                }
            //            }
            //        }

            //        newSolutionSegments.AddRange(segmentSegments);
            //    }

            //    routeSolution.Segments = newSolutionSegments;
            //}

            return new UavRouteAlgorithmOutput
            {
                Routes = bestSolution!
            };
        }

        private void PrepareInputData(UavRouteAlgorithmInput inputData) => _pheromonesMatrix = MatrixHelper.BuildDistanceMatrix(inputData.Targets);

        private UavInfo ChooseNextUav(UavInfo current, UavInfo lastUav, List<UavInfo> orderedUavs)
        {
            if (current.Uav.Id == lastUav.Uav.Id)
                return orderedUavs[0];

            var nextNotFinishedUAVs = orderedUavs
                   .SkipWhile(x => x.Uav.Id != current.Uav.Id)
                   .Skip(1)
                   .Where(x => !x.IsFinished)
                   .ToArray();

            if (nextNotFinishedUAVs.Any())
                return nextNotFinishedUAVs[0];

            return orderedUavs[0];
        }

        private bool CanReachEndPoint(TargetPair targetPair, UavInfo currentUavInfo)
        {
            var remainingResourceAfterPair = currentUavInfo.RemainingFlightResource - targetPair.RouteLength;

            var visited = new HashSet<Point>();
            var queue = new Queue<(Point position, double range)>();

            queue.Enqueue((targetPair.Targets.Point2, remainingResourceAfterPair));

            while (queue.Count > 0)
            {
                var (position, rangeLeft) = queue.Dequeue();
                if (visited.Contains(position)) continue;
                visited.Add(position);

                // Check if we can reach finish directly
                if (ClassicRouteBuilder.GetRouteLength(position, currentUavInfo.Uav.End) <= rangeLeft)
                    return true;

                // Check all service points we can reach
                foreach (var service in currentUavInfo.Uav.ServicePoints)
                {
                    if (!visited.Contains(service) &&
                        ClassicRouteBuilder.GetRouteLength(position, service) <= rangeLeft)
                    {
                        // Refuel to maxRange
                        queue.Enqueue((service, currentUavInfo.Uav.MaxDistanceRange));
                    }
                }
            }

            return false;
        }

        private (double totalDistance, RouteSegment segment) GetEndPointReachableShortestSegment(UavInfo currentUavInfo)
        {
            var lastUavSegment = currentUavInfo.Route.Segments[^1];
            var start = lastUavSegment.Points[^1].DisplayCoordinates;
            var finish = currentUavInfo.Uav.End;
            var allPoints = new List<Point> { start };
            allPoints.AddRange(currentUavInfo.Uav.ServicePoints);

            if (finish != start)
                allPoints.Add(finish);

            var distances = new Dictionary<Point, double>();
            var previous = new Dictionary<Point, Point?>();
            var visited = new HashSet<Point>();

            distances = allPoints.ToDictionary(p => p, _ => double.PositiveInfinity);
            distances[start] = 0;

            while (true)
            {
                // Select unvisited point with smallest distance
                var current = distances
                    .Where(d => !visited.Contains(d.Key))
                    .OrderBy(d => d.Value)
                    .Select(d => d.Key)
                    .FirstOrDefault();

                if (current == null || double.IsInfinity(distances[current]))
                    break; // no reachable unvisited nodes remain

                visited.Add(current);

                if (current == finish)
                    break; // reached the goal

                // Explore neighbors within maxRange
                foreach (var neighbor in allPoints.Where(p => p != current))
                {
                    var dist = ClassicRouteBuilder.GetRouteLength(current, neighbor);
                    if (dist > currentUavInfo.Uav.MaxDistanceRange) continue; // can't reach neighbor directly

                    double newDist = distances[current] + dist;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                    }
                }
            }

            if (double.IsInfinity(distances[finish]))
                return (0, null); // unreachable

            // Reconstruct path
            var path = new List<Point>();
            var cur = finish;
            while (cur != null)
            {
                path.Add(cur);
                previous.TryGetValue(cur, out cur);
            }

            path.Reverse();
            return (distances[finish], new ClassicRouteSegment(path.Select(x => new RouteSegmentPoint(x, x)).ToList()));
        }

        private Dictionary<(Point, Point), double> CalculateTransitionProbabilities(
                List<TargetPair> candidates,
                double alpha,
                double beta)
        {
            var probabilities = new Dictionary<(Point, Point), double>();
            double denominator = 0.0;

            foreach (var pair in candidates)
            {
                var p1 = pair.Targets.Point1;
                var p2 = pair.Targets.Point2;

                // знайти ребро в матриці (будь-який порядок)
                //var edge = _pheromonesMatrix.FirstOrDefault(x =>
                //    (x.Point1 == p1 && x.Point2 == p2) || (x.Point1 == p2 && x.Point2 == p1));

                double tau = 1e-6; // невелике значення якщо немає
                double dij = pair.RouteLength; // або можна використовувати edge.Distance

                if (dij <= 0) dij = 1e-6;

                double value = Math.Pow(tau, alpha) * Math.Pow(1.0 / dij, beta);
                probabilities[(p1, p2)] = value;
                denominator += value;
            }

            if (denominator > 0)
            {
                var keys = probabilities.Keys.ToList();
                foreach (var k in keys) probabilities[k] /= denominator;
            }

            return probabilities;
        }

        private void UpdatePheromones(
            List<Route> antRoutes,
            List<int> Wl,
            int Wmax,
            double rho)
        {
            // випаровування
            foreach (var edge in _pheromonesMatrix)
                edge.Pheromone *= (1.0 - rho);

            for (int l = 0; l < antRoutes.Count; l++)
            {
                var route = antRoutes[l];
                var delta = (double)Wl[l] / (double)Wmax;

                var points = route.GetAllPoints();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    var from = points[i].MapCoordinates; // переконайтесь, що це саме Point обʼєкт з матриці
                    var to = points[i + 1].MapCoordinates;

                    var edge = _pheromonesMatrix.FirstOrDefault(e =>
                        (e.Point1 == from && e.Point2 == to) || (e.Point1 == to && e.Point2 == from));

                    if (edge != null)
                    {
                        edge.Pheromone += delta;
                    }
                }
            }
        }

        private List<Point> GetVisiblePoints(
            List<Point> points,
            Point A,
            Point B,
            double R)
        {
            var visible = new List<(Point point, double t)>();

            foreach (var p in points)
            {
                double distance = DistancePointToSegment(p, A, B);

                if (distance <= R)
                {
                    // знайти параметр t (позицію точки вздовж відрізка A→B)
                    double t = GetProjectionParameter(p, A, B);
                    visible.Add((p, t));
                }
            }

            // Сортуємо точки по t — це і є порядок проходження маршруту
            return visible
                .OrderBy(v => v.t)
                .Select(v => v.point)
                .ToList();

            bool IsVisibleOnSegment(Point p, Point p1, Point p2, double R)
            {
                double d = DistancePointToSegment(p, p1, p2);
                return d <= R;
            }

           double DistancePointToSegment(Point p, Point p1, Point p2)
           {
                double vx = p2.Coordinates.X - p1.Coordinates.X;
                double vy = p2.Coordinates.Y - p1.Coordinates.Y;

                double wx = p.Coordinates.X - p1.Coordinates.X;
                double wy = p.Coordinates.Y - p1.Coordinates.Y;

                double c1 = wx * vx + wy * vy;
                if (c1 <= 0)
                    return Distance(p, p1);

                double c2 = vx * vx + vy * vy;
                if (c2 <= c1)
                    return Distance(p, p2);

                double t = c1 / c2;
                double projX = p1.Coordinates.X + t * vx;
                double projY = p1.Coordinates.Y + t * vy;

                return Math.Sqrt((p.Coordinates.X - projX) * (p.Coordinates.X - projX) +
                                 (p.Coordinates.Y - projY) * (p.Coordinates.Y - projY));
           }

            double Distance(Point a, Point b)
            {
                double dx = a.Coordinates.X - b.Coordinates.X;
                double dy = a.Coordinates.Y - b.Coordinates.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            double GetProjectionParameter(Point p, Point A, Point B)
            {
                double vx = B.Coordinates.X - A.Coordinates.X;
                double vy = B.Coordinates.Y - A.Coordinates.Y;

                double wx = p.Coordinates.X - A.Coordinates.X;
                double wy = p.Coordinates.Y - A.Coordinates.Y;

                double c1 = wx * vx + wy * vy;
                double c2 = vx * vx + vy * vy;

                if (c2 == 0) return 0; // A == B

                double t = c1 / c2;

                // обмежуємо в межах відрізка
                if (t < 0) t = 0;
                if (t > 1) t = 1;

                return t;
            }
        }

        private double GetProjectionT(Point p, Point a, Point b)
        {
            double ABx = b.Coordinates.X - a.Coordinates.X;
            double ABy = b.Coordinates.Y - a.Coordinates.Y;

            double APx = p.Coordinates.X - a.Coordinates.X;
            double APy = p.Coordinates.Y - a.Coordinates.Y;

            double dotAP_AB = APx * ABx + APy * ABy;
            double dotAB_AB = ABx * ABx + ABy * ABy;

            if (dotAB_AB == 0)
                return 0; // A==B, no direction

            return dotAP_AB / dotAB_AB;
        }

        private void EnhanceUAVFinishSegment(List<UavInfo> uavOrder, List<Point> availablePoints)
        {
            foreach (var uav in uavOrder)
            {
                var lastUavSegment = uav.Route.Segments[^1];

                if (lastUavSegment is not ClassicRouteSegment)
                    return;

                var preLastUavSegment = uav.Route.Segments[^2];
                var startPoint = preLastUavSegment.Points[0];
                var centerPoint = preLastUavSegment.Points[^1];

                var chosenType = 0;

                if (preLastUavSegment is ClassicRouteSegment && preLastUavSegment.Points[^1].Type != PointType.Service)
                {
                    chosenType = 1;
                    var newSmoothedSegment = SmoothedRouteBuilder.BuildSmoothedRouteSegment(
                           startPoint.DisplayCoordinates,
                           centerPoint.DisplayCoordinates,
                           uav.Uav.End,
                           uav.Uav.RecognitionRadius);

                    if (preLastUavSegment.Points.Count <= 2)
                    {
                        chosenType = 11;

                        uav.Route.Segments.RemoveAt(uav.Route.Segments.Count - 1);
                        uav.Route.Segments.RemoveAt(uav.Route.Segments.Count - 1);

                        uav.Route.Segments.Add(newSmoothedSegment);
                    }
                    // any points were observer dгring the classic route
                    else
                    {
                        chosenType = 12;

                        var previouslyObservedCount = preLastUavSegment.Points.Count() - 2;

                        var preLastUavSegmentCopy = new ClassicRouteSegment(preLastUavSegment.Points.Select(x => x).ToList());
                        preLastUavSegmentCopy.Points.RemoveAt(0);
                        preLastUavSegmentCopy.Points.RemoveAt(preLastUavSegmentCopy.Points.Count - 1);
                        var nowAvailablePoints = availablePoints.Concat(preLastUavSegmentCopy.Points.Select(x => x.MapCoordinates)).ToList();

                        var nowObserverToSmoothPoint = GetVisiblePoints(nowAvailablePoints, newSmoothedSegment.Points[0].DisplayCoordinates, newSmoothedSegment.Points[1].DisplayCoordinates, uav.Uav.RecognitionRadius);
                        var nowObserverFromSmoothPoint = GetVisiblePoints(nowAvailablePoints, newSmoothedSegment.Points[1].DisplayCoordinates, newSmoothedSegment.Points[2].DisplayCoordinates, uav.Uav.RecognitionRadius);

                        if (nowObserverToSmoothPoint.Count + nowObserverFromSmoothPoint.Count >= previouslyObservedCount)
                        {
                            // TODO: add previouslyobserver during the classic route point into the smoothed new route

                            uav.Route.Segments.RemoveAt(uav.Route.Segments.Count - 1);
                            uav.Route.Segments.RemoveAt(uav.Route.Segments.Count - 1);

                            uav.Route.Segments.Add(newSmoothedSegment);
                        }
                    }
                }

                if (preLastUavSegment is SmoothedRouteSegment && 
                    preLastUavSegment.Points[^1].MapCoordinates == preLastUavSegment.Points[^1].DisplayCoordinates)
                {
                    chosenType = 2;
                    //preLastUavSegment.Points.RemoveAt(preLastUavSegment.Points.Count - 1);
                    //preLastUavSegment.Points.RemoveAt(preLastUavSegment.Points.Count - 1);

                    var newSmoothedSegment = SmoothedRouteBuilder.BuildSmoothedRouteSegment(
                        startPoint.DisplayCoordinates,
                        centerPoint.DisplayCoordinates,
                        uav.Uav.End,
                        uav.Uav.RecognitionRadius);

                    // TODO: what if any points were observer dгring the smoothed route?

                    uav.Route.Segments.RemoveAt(uav.Route.Segments.Count - 1);

                    uav.Route.Segments.Add(newSmoothedSegment);
                }

                if (uav.Route.Segments.Any(x => x.Points.Count == 1))
                {
                    Console.WriteLine("");
                }
            }
        }
    }
}
