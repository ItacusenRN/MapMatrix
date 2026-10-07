namespace MapMatrix.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _env;
        
        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> saveImageAsync(IFormFile file)
        {
            if(file == null || file.Length == 0)
                throw new ArgumentException("Fichier vide.");

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(!allowed.Contains(ext))
                throw new ArgumentException("Formats non autorisés: jpg, jpeg, png, webp.");

            if(file.Length > 5 * 1024 * 1024) // 5MB
                throw new ArgumentException("Fichier trop volumineux. Maximum: 5MB.");

            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
            Directory.CreateDirectory(uploads);

            var filename = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(uploads, filename);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            // URL relative servie par UseStaticFiles
            return $"/uploads/{filename}";
        }
    }
}
