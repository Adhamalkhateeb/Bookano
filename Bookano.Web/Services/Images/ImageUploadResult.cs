namespace Bookano.Web.Services.Images
{
    public class ImageUploadResult
    {
        public bool IsSuccess { get; set; }
        public string? Url { get; set; }
        public string? PublicId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
