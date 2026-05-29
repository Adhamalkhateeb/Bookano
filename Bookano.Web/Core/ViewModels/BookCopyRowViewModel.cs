namespace Bookano.Web.Core.ViewModels
{
    public class BookCopyRowViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public bool IsAvailableForRental { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
    }
}
