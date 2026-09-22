namespace MapMatrix.DT0s
{
    public class SendPositionRequest
    {
        public Guid sessionId { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public decimal? altitude { get; set; }
        public decimal? accuracy { get; set; }
        public decimal? speed { get; set; }
        public decimal? heading { get; set; }
    }
}
