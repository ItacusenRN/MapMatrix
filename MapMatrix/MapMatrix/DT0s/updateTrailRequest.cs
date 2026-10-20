namespace MapMatrix.DT0s
{
    public class updateTrailRequest
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? coverImageUrl { get; set; }
        public string? transportMode { get; set; }
        public short? difficulty { get; set; }
        public decimal? distanceKm { get; set; }
        public string? estimatedDuration { get; set; }
        public List<List<double>>? coordinates { get; set; }
    }
}
