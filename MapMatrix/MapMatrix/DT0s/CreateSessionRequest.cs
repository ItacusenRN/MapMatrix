namespace MapMatrix.DT0s
{
    public class CreateSessionRequest
    {
        public Guid trailId { get; set;  }
        public Guid? scheduleId { get; set; }
        public int participantsCount { get; set; } = 0;
        public DateTime? plannedStart { get; set; }
        public string? notes { get; set; }
    }
}
