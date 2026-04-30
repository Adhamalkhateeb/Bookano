using Bookano.Web.Services.Image.Result;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Bookano.Web.Services.Image
{
    public sealed class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public LocalImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task<ImageDeleteResult> DeleteAsync(string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId))
            {
                return Task.FromResult(
                    new ImageDeleteResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Image id is required.",
                    }
                );
            }

            var path = Path.Combine(_env.WebRootPath, "images", imageId);

            try
            {
                if (!File.Exists(path))
                {
                    return Task.FromResult(
                        new ImageDeleteResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Image not found.",
                        }
                    );
                }

                File.Delete(path);

                return Task.FromResult(new ImageDeleteResult { IsSuccess = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new ImageDeleteResult { IsSuccess = false, ErrorMessage = ex.Message }
                );
            }
        }

        public string GetThumbnail(string imageId, int width = 125) => $"/images/{imageId}";

        public async Task<Result.ImageUploadResult> UploadAsync(
            IFormFile file,
            string folder,
            string? fileName
        )
        {
            var validateImageResult = ValidateImage(file);
            if (validateImageResult is not null)
                return new() { IsSuccess = false, ErrorMessage = validateImageResult };

            fileName = fileName ?? Path.GetFileName(fileName);

            var dir = Path.Combine(_env.WebRootPath, "images", folder);

            Directory.CreateDirectory(dir);

            var fullPath = Path.Combine(dir, fileName);

            using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream);

            var publicId = $"{folder}/{fileName}";

            return new Result.ImageUploadResult
            {
                IsSuccess = true,
                Url = $"/images/{publicId}",
                PublicId = publicId,
            };
        }

        public string? ValidateImage(IFormFile file) => Core.Consts.Image.ValidateImage(file);
    }
}
