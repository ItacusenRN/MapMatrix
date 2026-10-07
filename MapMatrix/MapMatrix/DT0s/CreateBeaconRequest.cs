namespace MapMatrix.DT0s
{
    public class CreateBeaconRequest
    {
        public Guid trailId { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int sequenceOrder { get; set; } = 0;
        public bool estReference { get; set; } = false;
    }
}
