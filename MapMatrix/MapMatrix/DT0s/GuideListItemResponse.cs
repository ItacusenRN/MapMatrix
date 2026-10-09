namespace MapMatrix.DT0s
{
    public class GuideListItemResponse
    {
        public Guid id { get; set; }
        public string email { get; set; } = null!;
        public string firstName { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string? phone { get; set; }
        public bool isActive { get; set; }
        public bool isAvailable { get; set; }
        public Guid? currentAssignmentId { get; set; }
        public Guid? currentSessionId { get; set; }
        public string? currentTrailName { get; set; }
    }
}
