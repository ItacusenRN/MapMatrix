namespace MapMatrix.DT0s
{
    public class TriggerEmergencyRequest
    {
        public Guid sessionId { get; set; }

        public double longitude { get; set; }
        public double latitude { get; set; }
        public string? message { get; set; }
    }
}
