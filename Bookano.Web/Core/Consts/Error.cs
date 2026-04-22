namespace Bookano.Web.Core.Consts
{
    public static class Error
    {
        public const string MaxLength = "Length cannot be more than {1} characters.";
        public const string Duplicated = "{0} with the same value already exists!";
        public const string NotAllowedImageExtension =
            "Only (.jpg, .jpeg, .png, .webp) files are allowed";
        public const string ImageMaxSizeLimit = "File cannot exceed 2MB";
        public const string NotAllowFutureDates = "Date cannot be in the future!";
        public const string ShouldBeInRange = "{0} should be between {1} and {2}!";
    }
}
