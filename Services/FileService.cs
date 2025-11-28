using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Services.IService;

namespace Services
{
    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _environment;

        public FileService(IHostingEnvironment env)
        {
            _environment = env;
        }

        public Tuple<int, string> SaveImage(IFormFile imageFile)
        {
            try
            {
                var wwwPath = _environment.WebRootPath;
                var path = Path.Combine(wwwPath, "Uploads");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Allowed extensions
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                var allowedExtensions = new[]
                {
                    ".jpg", ".jpeg", ".png", ".docx", ".doc", ".pdf", ".pptx", ".mp3", ".mp4"
                };

                if (!allowedExtensions.Contains(ext))
                {
                    var msg = $"Only the following extensions are allowed: {string.Join(", ", allowedExtensions)}";
                    return Tuple.Create(0, msg);
                }

                var uniqueName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(path, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                return Tuple.Create(1, uniqueName);
            }
            catch
            {
                return Tuple.Create(0, "An error occurred while saving the file.");
            }
        }

        public bool DeleteImage(string imageFileName)
        {
            try
            {
                var wwwPath = _environment.WebRootPath;
                var fullPath = Path.Combine(wwwPath, "Uploads", imageFileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
