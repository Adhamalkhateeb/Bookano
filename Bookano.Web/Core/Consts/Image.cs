namespace Bookano.Web.Core.Consts
{
    public static class Image
    {
        public const int MaxSize = 2 * 1024 * 1024;
        public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        public static string? ValidateImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Image.AllowedExtensions.Contains(ext))
                return Core.Consts.Error.NotAllowedImageExtension;

            if (file.Length > Image.MaxSize)
                return Core.Consts.Error.ImageMaxSizeLimit;

            return null;
        }
    }
}
