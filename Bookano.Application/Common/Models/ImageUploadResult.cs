namespace Bookano.Application.Common.Models;

public class ImageUploadResult 
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Url { get; set; }
    public string? PublicId { get; set; }
}
