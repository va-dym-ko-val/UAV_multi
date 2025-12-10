namespace WebServer.Models
{
    public class RouteOutputModel
    {
        public PointUavModel Uav { get; set; }
        public List<RouteSegmentModel> Segments { get; set; } = new();
    }
}
