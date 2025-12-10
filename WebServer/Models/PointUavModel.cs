namespace WebServer.Models
{
    public class PointUavModel
    {
        public int Id { get; set; }
        public PointModel Start { get; set; }
        public PointModel End { get; set; }
        public List<PointModel> ServicePoints { get; set; }
        public double MaxDistanceRange { get; set; }
        public double RecognitionRadius { get; set; }
    }
}
