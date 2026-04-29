using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace Bookano.Web.Services.Images
{
    public sealed class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageService(IOptions<CloudinarySettings> options)
        {
            var account = new Account(
                options.Value.Cloud,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task DeleteAsync(string imageId)
        {
            await _cloudinary.DestroyAsync(new DeletionParams(imageId));
        }

        public string GetThumbnail(string imageId, int width = 125)
        {
            return _cloudinary
                .Api.UrlImgUp.Transform(
                    new Transformation()
                        .Width(width)
                        .Crop("scale")
                        .Quality("auto")
                        .FetchFormat("auto")
                )
                .BuildUrl(imageId);
        }

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

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UniqueFilename = true,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
            {
                return new ImageUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = result.Error.Message,
                };
            }

            return new ImageUploadResult
            {
                IsSuccess = true,
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId,
            };
        }
    }
}
