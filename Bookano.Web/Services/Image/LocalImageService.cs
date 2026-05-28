using Bookano.Web.Services.Image.Result;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Bookano.Web.Services.Image
{
    public sealed class LocalImageService(IWebHostEnvironment env) : IImageService
    {
        private readonly IWebHostEnvironment _env = env;
        private const string ImagesRoot = "images";
        private const string ThumbFolder = "thumb";

        public async Task<ImageDeleteResult> DeleteAsync(string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId))
                return FailDelete("Image id is required.");

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, ImagesRoot, imageId);
                var thumbPath = GetThumbnailPath(imageId);

                if (!File.Exists(filePath))
                    return FailDelete("Image not found.");

                await Task.Run(() => File.Delete(filePath));

                if (File.Exists(thumbPath))
                    await Task.Run(() => File.Delete(thumbPath));

                return new ImageDeleteResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return FailDelete(ex.Message);
            }
        }

        public async Task<ImageUploadResult> UploadAsync(
            IFormFile file,
            string folder,
            string? fileName
        )
        {
            var validationError = ValidateImage(file);
            if (validationError is not null)
                return new() { IsSuccess = false, ErrorMessage = validationError };

            fileName ??= Path.GetFileName(file.FileName);

            var dir = Path.Combine(_env.WebRootPath, ImagesRoot, folder);
            var thumbDir = Path.Combine(_env.WebRootPath, ImagesRoot, folder, ThumbFolder);

            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(thumbDir);

            var filePath = Path.Combine(dir, fileName);
            var thumbPath = Path.Combine(thumbDir, fileName);

            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            await CreateThumbnailAsync(filePath, thumbPath, 125);

            var publicId = $"{folder}/{fileName}";

            return new ImageUploadResult
            {
                IsSuccess = true,
                Url = GetUrl(publicId),
                PublicId = publicId,
            };
        }

        public string GetThumbnail(string imageId, int width = 125)
        {
            var thumbPath = GetThumbnailPath(imageId);

            return File.Exists(thumbPath)
                ? GetUrl(
                    $"{Path.GetDirectoryName(imageId) ?? ""}/{ThumbFolder}/{Path.GetFileName(imageId)}"
                )
                : GetUrl(imageId);
        }

        public string? ValidateImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Domain.Common.Constants.Image.AllowedExtensions.Contains(ext))
                return Error.NotAllowedImageExtension;

            if (file.Length > Domain.Common.Constants.Image.MaxSize)
                return Error.ImageMaxSizeLimit;

            return null;
        }

        private static async Task CreateThumbnailAsync(string source, string destination, int width)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(source);

            var height = (int)(image.Height * (width / (float)image.Width));

            image.Mutate(x => x.Resize(width, height));

            await image.SaveAsync(destination);
        }

        private string GetThumbnailPath(string imageId)
        {
            var folder = Path.GetDirectoryName(imageId) ?? "";
            var fileName = Path.GetFileName(imageId);

            return Path.Combine(_env.WebRootPath, ImagesRoot, folder!, ThumbFolder, fileName);
        }

        private static string GetUrl(string relativePath) =>
            $"/{ImagesRoot}/{relativePath.Replace("\\", "/")}";

        private static ImageDeleteResult FailDelete(string message) =>
            new() { IsSuccess = false, ErrorMessage = message };
    }
}
