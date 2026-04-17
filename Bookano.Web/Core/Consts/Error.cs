namespace Bookano.Web.Core.Consts
{
    public static class Error
    {
        public const string MaxLength = "Length cannot be more than {1} characters.";
        public const string Duplicated = "{0} with the same name already exists!";
        public const string NotAllowedImageExtension =
            "Only (.jpg, .jpeg, .png, .webp) files are allowed";

        public const string ImageMaxSizeLimit = "File cannot exceed 2MB";
    }
}
