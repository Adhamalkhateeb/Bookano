namespace Bookano.Domain.Entities
{
    public sealed class Governorate : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}
