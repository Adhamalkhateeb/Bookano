namespace Bookano.Application.Common.Models;

public sealed class ImageUploadDto
{
    public Stream Stream { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public long Length { get; set; }
}
