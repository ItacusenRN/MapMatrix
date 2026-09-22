namespace MapMatrix.DT0s
{
    public class SessionResponse
    {
        public Guid id { get; set; }
        public Guid guideId { get; set; }
        public Guid trailId { get; set; }
        public string trailName { get; set; } = null!;
        public string status { get; set; } = null!;
        public int participantsCount { get; set; }
        public DateTime? startedAt { get; set; }
        public DateTime? endedAt { get; set; }
        public DateTime? plannedStart { get; set; }
        public string? notes { get; set; }
        public DateTime createdAt { get; set; }
    }
}
