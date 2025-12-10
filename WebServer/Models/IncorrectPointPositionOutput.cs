namespace WebServer.Models
{
    public class IncorrectPointPositionOutput
    {
        public int UavId { get; set; }
        public IEnumerable<PointModel> Points {  get; set; }
    }
}
