namespace MapMatrix.DT0s
{
    public class EmergencyResponse
    {
        public Guid id { get; set; }
        public Guid sessionId { get; set; }
        public Guid guideId { get; set; }
        public string guideName { get; set; } = null!;
        public string trailName { get; set; } = null!;
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string? message { get; set; }
        public string status { get; set; } = null!;
        public Guid? handledBy { get; set; }
        public DateTime triggeredAt { get; set; }
        public DateTime? resolvedAt { get; set; }
    }
}
