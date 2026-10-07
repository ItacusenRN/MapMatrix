namespace MapMatrix.DT0s
{
    public class CreateContentRequest
    {
        public string type { get; set; } = "news"; // news, event, ad, organization
        public string title { get; set; } = null!;
        public string? slug { get; set; }
        public string? summary { get; set; }
        public string? body { get; set; }
        public string? imageUrl { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public Guid? trailId { get; set; }
        public bool publishNow { get; set; } = false;
    }
}
