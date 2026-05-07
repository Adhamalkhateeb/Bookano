namespace Bookano.Web.Core.ViewModels
{
    public class SubscriberSearchResultViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string ImageThumbnailUrl { get; set; } = null!;
    }
}
