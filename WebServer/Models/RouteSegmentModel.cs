namespace WebServer.Models
{
    public class RouteSegmentModel
    {
        public List<RouteSegmentPointOutputModel> Points { get; set; } = new();
        public double Length { get; set; }
        public RouteSegmentType SegmentType { get; set; }
    }

    public enum RouteSegmentType
    {
        Classic,
        Smoothed
    }
}
