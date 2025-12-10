using RouteOptimizer.Models;

namespace WebServer.Models
{
    public class RouteSegmentPointOutputModel
    {
        public PointModel MapCoordinates { get; set; }
        public PointModel DisplayCoordinates { get; set; }
        public PointType Type { get; set; }
    }
}
