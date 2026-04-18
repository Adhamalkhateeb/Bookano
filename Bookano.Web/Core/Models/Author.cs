namespace Bookano.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class Author : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<BookAuthor> Books { get; set; } = [];
    }
}
