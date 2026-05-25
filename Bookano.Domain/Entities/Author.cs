namespace Bookano.Domain.Entities
{
    public sealed class Author : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<BookAuthor> Books { get; set; } = [];
    }
}
