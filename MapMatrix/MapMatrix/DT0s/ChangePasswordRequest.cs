namespace MapMatrix.DT0s
{
    public class ChangePasswordRequest
    {
        public string currentPassword { get; set; } = null!;
        public string newPassword { get; set; } = null!;
    }
}
