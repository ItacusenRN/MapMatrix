namespace MapMatrix.DT0s
{
    public class AssignmentResponse
    {
        public Guid id { get; set; }
        public Guid trailId { get; set; }
        public string trailName { get; set; } = null!;
        public Guid guideId { get; set; }
        public string guideName { get; set; } = null!;
        public Guid? assignedBy { get; set; }
        public string status { get; set; } = null!;
        public bool requestedByGuide { get; set; }
        public DateTime? scheduledDate { get; set; }
        public string? notes { get; set; }
        public DateTime createdAt { get; set; }
    }
}
