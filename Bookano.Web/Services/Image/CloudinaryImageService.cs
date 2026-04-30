using System.Net;
using Bookano.Web.Services.Image.Result;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace Bookano.Web.Services.Image
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

        public async Task<ImageDeleteResult> DeleteAsync(string imageId)
        {
            if (string.IsNullOrWhiteSpace(imageId))
            {
                return new ImageDeleteResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Image id is required.",
                };
            }

            var deletionResult = await _cloudinary.DestroyAsync(new DeletionParams(imageId));

            if (deletionResult.Error is not null)
            {
                return new ImageDeleteResult
                {
                    IsSuccess = false,
                    ErrorMessage = deletionResult.Error.Message,
                };
            }

            if (deletionResult.StatusCode == HttpStatusCode.OK)
            {
                return new ImageDeleteResult { IsSuccess = true };
            }

            if (deletionResult.StatusCode == HttpStatusCode.NotFound)
            {
                return new ImageDeleteResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Image not found.",
                };
            }

            return new ImageDeleteResult
            {
                IsSuccess = false,
                ErrorMessage = $"Deletion failed: {deletionResult.Result}",
            };
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

        public async Task<Result.ImageUploadResult> UploadAsync(
            IFormFile file,
            string folder,
            string? fileName
        )
        {
            var validateImageResult = ValidateImage(file);
            if (validateImageResult is not null)
                return new() { IsSuccess = false, ErrorMessage = validateImageResult };

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName ?? file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
            {
                return new Result.ImageUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = result.Error.Message,
                };
            }

            return new Result.ImageUploadResult
            {
                IsSuccess = true,
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId,
            };
        }

        public string? ValidateImage(IFormFile file) => Core.Consts.Image.ValidateImage(file);
    }
}
