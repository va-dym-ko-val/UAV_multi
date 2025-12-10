using RouteOptimizer.Models;

namespace WebServer.Models
{
    public class PointModel
    {
        public float X { get; set; }
        public float Y { get; set; }
        public PointType? Type { get; set; }
    }
}
