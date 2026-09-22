namespace MapMatrix.DT0s
{
    public class CreateGuideReponse
    {
        public Guid identifiantGuide { get; set; }
        public string email { get; set; } = null!;
        public string nom { get; set; } = null!;
        public string prenom { get; set; } = null!;
        public string generatePassword { get; set; } = null!;
    }
}
