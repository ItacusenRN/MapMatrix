namespace MapMatrix.DT0s
{
    public class CreateGuideRequest
    {
        public string email { get; set; } = null!;
        public string firstName { get; set; } = null!;
        public string lastName { get; set; } = null!;
        public string? phone { get; set; }
    }
}
