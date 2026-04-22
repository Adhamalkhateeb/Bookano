namespace Bookano.Web.Core.ViewModels
{
    public class BookCopyViewModel
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = null!;
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public bool IsAvailableForRental { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
    }
}
