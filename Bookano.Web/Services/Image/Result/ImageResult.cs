namespace Bookano.Web.Services.Image.Result
{
    public abstract class ImageResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
