namespace Bookano.Web.Core.ViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
