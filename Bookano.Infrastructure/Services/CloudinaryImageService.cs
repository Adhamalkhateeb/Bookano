using System.Net;
using Bookano.Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Error = Bookano.Application.Constants.Error;

namespace Bookano.Infrastructure.Services
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



        public async Task<Application.Common.Models.ImageUploadResult> UploadAsync(Stream stream, string originalFileName, string folder, string? fileName = null,
            CancellationToken ct = default
        )
        {
            var validationResult = ValidateImage(originalFileName, stream.Length);

            if (validationResult is not null)
                return new() { IsSuccess = false, ErrorMessage = validationResult };

            fileName ??= Path.GetFileName(originalFileName);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
            {
                return new Application.Common.Models.ImageUploadResult
                {
                    IsSuccess = false,
                    ErrorMessage = result.Error.Message,
                };
            }

            return new Application.Common.Models.ImageUploadResult
            {
                IsSuccess = true,
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId,
            };
        }


        public async Task<ImageDeleteResult> DeleteAsync(
            string imageId,
            CancellationToken ct = default)
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

            if (deletionResult.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound)
            {
                return new ImageDeleteResult
                {
                    IsSuccess = true,
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
                        .FetchFormat("auto"))
                .BuildUrl(imageId);
        }


        public string? ValidateImage(string originalFileName, long fileSize)
        {
            var extension = Path
                .GetExtension(originalFileName)
                .ToLowerInvariant();

            if (!Image.AllowedExtensions.Contains(extension))
               return Error.NotAllowedImageExtension;

            if (fileSize > Image.MaxSize)
                return Error.ImageMaxSizeLimit;

            return null;
        }
    }
}
