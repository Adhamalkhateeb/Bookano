using Bookano.Web.Services.Image.Result;

namespace Bookano.Web.Services.Image
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadAsync(IFormFile file, string folder, string? fileName);
        Task<ImageDeleteResult> DeleteAsync(string imageId);
        string GetThumbnail(string imageId, int width = 125);
        string? ValidateImage(IFormFile file);
    }
}
