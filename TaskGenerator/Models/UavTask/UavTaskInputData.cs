namespace TaskGenerator.Models.UavTask
{
    public class UavTaskInputData : TaskInputData
    {
        public required int TargetsNumber { get; set; }
        public required UavConfiguration[] UavsConfiguration { get; set; }
        public required CoordinatesRange XCoordinates { get; set; }
        public required CoordinatesRange YCoordinates { get; set; }

        public override string TaskName { get => "UAVTask"; set => throw new NotImplementedException(); }
    }

    public class CoordinatesRange
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }

    public class UavConfiguration
    {
        public int ServicePointsNumber { get; set; }
        public float DistanceResource { get; set; }
        public float RecognitionRadius { get; set; }
    }
}
