namespace MapMatrix.DT0s
{
    public class updateContentRequest
    {
        public string? type { get; set; }
        public string? title { get; set; }
        public string? slug { get; set; }
        public string? summary { get; set; }
        public string? body { get; set; }
        public string? imageUrl { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public Guid? trailId { get; set; }
    }
}
