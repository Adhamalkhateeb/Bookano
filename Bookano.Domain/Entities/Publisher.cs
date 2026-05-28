namespace Bookano.Domain.Entities;

public sealed class Publisher : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = [];
}
