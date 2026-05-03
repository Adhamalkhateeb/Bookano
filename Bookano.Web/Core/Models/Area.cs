namespace Bookano.Web.Core.Models
{
    [Index(nameof(Name), nameof(GovernorateId), IsUnique = true)]
    public sealed class Area : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
    }
}
