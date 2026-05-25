namespace Bookano.Web.Extensions
{
    public static class UserExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal) =>
            principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
