namespace MapMatrix.DT0s
{
    public class BeaconResponse
    {
        public Guid id { get; set; }
        public Guid trailId { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int sequenceOrder { get; set; }
        public bool estReference { get; set; }
        public DateTime createdAt { get; set; }
    }
}
