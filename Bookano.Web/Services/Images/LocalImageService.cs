using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Bookano.Web.Services.Images
{
    public sealed class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public LocalImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task DeleteAsync(string imageId)
        {
            var path = Path.Combine(_env.WebRootPath, "images", imageId);

            if (File.Exists(path))
                File.Delete(path);

            return Task.CompletedTask;
        }

        public string GetThumbnail(string imageId, int width = 125) => $"/images/{imageId}";

        public async Task<ImageUploadResult> UploadAsync(IFormFile file, string folder)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!Image.AllowedExtensions.Contains(ext))
                return new()
                {
                    IsSuccess = false,
                    ErrorMessage = Core.Consts.Error.NotAllowedImageExtension,
                };

            if (file.Length > Image.MaxSize)
                return new()
                {
                    IsSuccess = false,
                    ErrorMessage = Core.Consts.Error.ImageMaxSizeLimit,
                };

            var fileName = $"{Guid.NewGuid()}{ext}";
            var dir = Path.Combine(_env.WebRootPath, "images", folder);

            Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, fileName);

            using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream);

            var publicId = $"{folder}/{fileName}";

            return new ImageUploadResult
            {
                IsSuccess = true,
                Url = $"/images/{publicId}",
                PublicId = publicId,
            };
        }
    }
}
