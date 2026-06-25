
namespace Bookano.Application.DTOs.BookCopies
{
    public class BookCopySaveDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int EditionNumber { get; set; }
        public bool IsAvailableForRental { get; set; }
    }
}
