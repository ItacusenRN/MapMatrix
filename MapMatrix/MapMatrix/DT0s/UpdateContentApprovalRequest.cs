namespace MapMatrix.DT0s
{
    public class UpdateContentApprovalRequest
    {
        public string approvalStatus { get; set; } = null!; // approved, rejected
        public bool publish { get; set; } = false;
    }
}
