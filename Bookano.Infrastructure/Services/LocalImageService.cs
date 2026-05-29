using Bookano.Application.Common.Models;
using Bookano.Application.Constants;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Bookano.Web.Services.Image
{
    public sealed class LocalImageService(IWebHostEnvironment env) : IImageService
    {
        private readonly IWebHostEnvironment _env = env;
        private const string ImagesRoot = "images";
        private const string ThumbFolder = "thumb";
        private readonly string[] _imageAllowedExtensions = ["png", "jpg", "jpeg","webp"];


        public async Task<ImageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string folder,
            string? fileName = null,
            CancellationToken ct = default )
        {
            var validationResult = ValidateImage( originalFileName,stream.Length);

            if (!string.IsNullOrEmpty(validationResult))
                return new() { IsSuccess = false, ErrorMessage = validationResult };


            fileName ??= Path.GetFileName(originalFileName);

            var directory = Path.Combine(
                _env.WebRootPath,
                ImagesRoot,
                folder);

            var thumbDirectory = Path.Combine(
                _env.WebRootPath,
                ImagesRoot,
                folder,
                ThumbFolder);

            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(thumbDirectory);

            var filePath = Path.Combine(directory, fileName);

            var thumbPath = Path.Combine(
                thumbDirectory,
                fileName);

            await using (var fileStream = File.Create(filePath))
            {
                stream.Position = 0;

                await stream.CopyToAsync(fileStream, ct);
            }

            await CreateThumbnailAsync(filePath,thumbPath,125,ct);

            var publicId = $"{folder}/{fileName}";

            return new ImageUploadResult
            {
                IsSuccess = true,
                Url = GetUrl(publicId),
                PublicId = publicId,
            };
        }

        public async Task<ImageDeleteResult> DeleteAsync(string imageId, CancellationToken ct = default)
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

        public string GetThumbnail(string imageId, int width = 125)
        {
            var thumbPath = GetThumbnailPath(imageId);

            if (File.Exists(thumbPath))
            {
                var folder = Path.GetDirectoryName(imageId) ?? string.Empty;

                var fileName = Path.GetFileName(imageId);

                return GetUrl($"{folder}/{ThumbFolder}/{fileName}");
            }

            return GetUrl(imageId);
        }

       


        public string? ValidateImage(string originalFileName,long fileSize)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            if (!_imageAllowedExtensions.Contains(extension))
                return Error.NotAllowedImageExtension;

            if (fileSize > Domain.Common.Constants.Image.MaxSize)
                return Error.ImageMaxSizeLimit;

            return null;
        }

        private static async Task CreateThumbnailAsync(
            string source,
            string destination,
            int width,
            CancellationToken ct = default)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(source, ct);

            var height = (int)(image.Height * (width / (float)image.Width));

            image.Mutate(x => x.Resize(width, height));

            await image.SaveAsync(destination, ct);
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
