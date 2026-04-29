namespace Bookano.Web.Services.Images
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadAsync(IFormFile file, string folder);
        Task DeleteAsync(string imageId);
        string GetThumbnail(string imageId, int width = 125);
        string? ValidateImage(IFormFile file);
    }
}
