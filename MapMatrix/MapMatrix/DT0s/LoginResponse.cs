namespace MapMatrix.DT0s
{
    public class LoginResponse
    {
        public string token { get; set; } = null!;
        public Guid userId { get; set; }
        public string email { get; set; } = null!;
        public string nom { get; set; } = null!;
        public string prenom { get; set; } = null!;
        public string role { get; set; } = null!;
    }
}
