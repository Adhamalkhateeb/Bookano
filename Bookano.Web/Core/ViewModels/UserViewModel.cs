using NuGet.Protocol.Plugins;

namespace Bookano.Web.Core.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? LastUpdatedOn { get; set; }
    }
}
