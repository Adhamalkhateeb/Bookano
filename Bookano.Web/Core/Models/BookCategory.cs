namespace Bookano.Web.Core.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(BookId, CategoryId);
        }

        public override bool Equals(object? obj)
        {
            var bookCategory = obj as BookCategory;
            if (bookCategory is null)
                return false;
            return CategoryId == bookCategory.CategoryId && BookId == bookCategory.BookId;
        }
    }
}
