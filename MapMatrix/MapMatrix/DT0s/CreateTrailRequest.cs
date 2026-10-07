namespace MapMatrix.DT0s
{
    public class CreateTrailRequest
    {
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public string? coverImageUrl { get; set; }
        public string transportMode { get; set; } = "pied"; // pied, velo, moto, autre
        public short? difficulty { get; set; }
        public decimal? distanceKm { get; set; }
        public string? estimatedDuration { get; set; } // format "HH:mm:ss" ou "2.05:30:00"

        // Liste de points du traces  [[Longitude, Latitude], [Longitude, Latitude], ...]
        public List<List<double>> coordinates { get; set; } = new();

    }
}
