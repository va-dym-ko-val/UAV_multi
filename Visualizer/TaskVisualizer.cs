using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using RouteOptimizer.Algorithms.AntColonyAlgorithms.Models;
using RouteOptimizer.Models;
using Point = RouteOptimizer.Models.Point;

namespace TaskVisualizer
{
    public static class TaskVisualizer
    {
        public static void DrawRoutes(IEnumerable<Route> routes, List<Point> allPoints, string filename, int width, int height)
        {
            using var image = new Image<Rgba32>(width, height, Color.White);
            var font = SystemFonts.CreateFont("Arial", 12);

            foreach (var point in allPoints)
            {
                switch (point.Type)
                {
                    case PointType.Start:
                        image.Mutate(ctx => ctx.Fill(Color.Green,
                            new Polygon(new LinearLineSegment(
                                new PointF(point.Coordinates.X - 10, point.Coordinates.Y - 10),
                                new PointF(point.Coordinates.X, point.Coordinates.Y + 10),
                                new PointF(point.Coordinates.X + 10, point.Coordinates.Y + 10)
                                ))));
                        break;

                    case PointType.Finish:
                        image.Mutate(ctx => ctx.Fill(Color.Red,
                            new Polygon(new LinearLineSegment(
                                new PointF(point.Coordinates.X - 10, point.Coordinates.Y - 10),
                                new PointF(point.Coordinates.X, point.Coordinates.Y + 10),
                                new PointF(point.Coordinates.X + 10, point.Coordinates.Y + 10)
                                ))));
                        break;

                    case PointType.Target:
                        image.Mutate(ctx => ctx.Fill(Color.Blue,
                            new EllipsePolygon(point.Coordinates.X, point.Coordinates.Y, 3)));
                        var route = routes.FirstOrDefault(x => x.GetAllPoints().Select(x => x.MapCoordinates).Contains(point));
                        if (route is not null)
                        {
                            image.Mutate(ctx => ctx.Fill(Color.FromRgba(0, 0, 255, 64), new EllipsePolygon(point.Coordinates,
                                                   (float)route.Uav.RecognitionRadius)));
                        }
                        break;

                    case PointType.Service:
                        image.Mutate(ctx => ctx.Fill(Color.Violet,
                            new Polygon(new LinearLineSegment(
                                new PointF(point.Coordinates.X, point.Coordinates.Y - 5),
                                new PointF(point.Coordinates.X - 5, point.Coordinates.Y),
                                new PointF(point.Coordinates.X, point.Coordinates.Y + 5),
                                new PointF(point.Coordinates.X + 5, point.Coordinates.Y)
                                ))));
                        break;

                    default:
                        throw new InvalidOperationException("Point type is not defined to visualize!");
                };
            }

            var colors = new Color[4] { Color.Khaki, Color.Yellow, Color.Aqua, Color.Orange };
            var colorIndex = 0;

            foreach (var route in routes)
            {
                var color = colors[colorIndex % colors.Length];
                var pen = Pens.Solid(color, 2);

                var routePoints = route.GetAllPoints();
                for (var i = 1; i < routePoints.Count; i++)
                {
                    image.Mutate(ctx => ctx.DrawLine(pen, routePoints[i - 1].DisplayCoordinates.Coordinates, routePoints[i].DisplayCoordinates.Coordinates));
                }

                image.Mutate(ctx =>
                    ctx.DrawText($"UAV{route.Uav.Id}", font, color,
                        new PointF(route.Uav.Start.Coordinates.X + 5, route.Uav.Start.Coordinates.Y + 5)));

                colorIndex++;
            }

            image.Save(filename);
        }
    }
}
