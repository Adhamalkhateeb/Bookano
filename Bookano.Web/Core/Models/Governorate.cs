namespace Bookano.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class Governorate : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = null!;
    }
}
