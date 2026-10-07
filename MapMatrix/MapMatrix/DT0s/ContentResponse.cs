namespace MapMatrix.DT0s
{
    public class ContentResponse
    {
        public Guid id { get; set; }
        public string type { get; set; } = null!;
        public string title { get; set; } = null!;
        public string? slug { get; set; }
        public string? summary { get; set; }
        public string? body { get; set; }
        public string? imageUrl { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public bool isPublished { get; set; }
        public DateTime? publishedAt { get; set; }
        public Guid? trailId { get; set; }
        public string approvalStatus { get; set; } = null!;
        public Guid? authorId { get; set; }
        public DateTime createdAt { get; set; }
    }
}
