using Microsoft.AspNetCore.Identity;

namespace Bookano.Web.Core.Models
{
    [Index(nameof(Email), IsUnique = true), Index(nameof(UserName), IsUnique = true)]
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? LastUpdatedById { get; set; }
        public DateTimeOffset? LastUpdatedOnUtc { get; set; }
    }
}
