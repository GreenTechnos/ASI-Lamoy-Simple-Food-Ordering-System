using backend.Constants;

namespace backend.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        // Inject IWebHostEnvironment
        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), AppConstants.FileStorageServiceErrors.FileCannotBeNullOrEmpty);
            }

            // Path to wwwroot/uploads
            var uploadsFolder = Path.Combine(_env.WebRootPath, subfolder);
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the web-accessible path
            return $"{AppConstants.FileStorageServicePaths.PathSeparator}{subfolder}{AppConstants.FileStorageServicePaths.PathSeparator}{fileName}";
        }
    }
}