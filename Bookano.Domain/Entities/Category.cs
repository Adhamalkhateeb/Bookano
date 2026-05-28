namespace Bookano.Domain.Entities;

public sealed class Category : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<BookCategory> Books { get; set; } = [];
}
