using System.Net;

namespace MapMatrix.DT0s
{
    public class TrailResponse
    {
        public Guid id { get; set; }
        public string name { get; set; } = null;
        public string? description { get; set; }
        public string? coverImageUrl { get; set; }
        public string transportMode { get; set; } = null;
        public short? difficulty { get; set; }
        public decimal? distanceKm { get; set; }
        public string? estimatedDuration { get; set; }
        public string status { get; set; } = null;
        public string approvalStatus { get; set; } = null;
        public Guid createdBy { get; set; }
        public DateTime? publishedAt { get; set; }
        public DateTime? createdAt { get; set; }

        // GeoJSON simplifie du tracé
        public object? geometry { get; set; }
    }
}
