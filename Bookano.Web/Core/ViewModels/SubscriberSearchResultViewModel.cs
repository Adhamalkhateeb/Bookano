namespace Bookano.Web.Core.ViewModels
{
    public class SubscriberSearchResultViewModel
    {
        public string? Key { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string ImageThumbnailUrl { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;
    }
}
