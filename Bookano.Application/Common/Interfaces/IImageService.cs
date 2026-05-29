namespace Bookano.Application.Common.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult> UploadAsync(
        Stream stream,
        string originalFileName,
        string folder,
        string? fileName = null,
        CancellationToken ct = default);

    Task<ImageDeleteResult> DeleteAsync(
        string imageId,
        CancellationToken ct = default);

    string GetThumbnail(
        string imageId,
        int width = 125);

    string? ValidateImage(
        string originalFileName,
        long fileSize);
}
