namespace MapMatrix.DT0s
{
    public class CreateAssignmentRequest
    {
        public Guid trailId { get; set; }
        public Guid guideId { get; set; }
        public DateTime? scheduledDate { get; set; }
        public string? notes { get; set; }
    }
}
