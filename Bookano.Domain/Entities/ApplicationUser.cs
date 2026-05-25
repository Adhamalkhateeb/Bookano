namespace Bookano.Domain.Entities
{
    public sealed class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public string? LastUpdatedById { get; set; }
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
