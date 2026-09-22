namespace MapMatrix.DT0s
{
    public class PositionResponse
    {
        public Guid id { get; set; }
        public Guid sessionId { get; set; }
        public Guid guideId { get; set; }
        public string guideName { get; set; } = null!;
        public double longitude { get; set; }
        public double latitude { get; set; }
        public decimal? altitude { get; set; }
        public decimal? accuracy { get; set; }
        public decimal? speed { get; set; }
        public decimal? heading { get; set; }
        public DateTime recordedAt { get; set; }
    }
}
