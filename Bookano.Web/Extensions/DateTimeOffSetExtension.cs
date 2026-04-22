using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Bookano.Web.Extensions
{
    public static class DateTimeOffSetExtension
    {
        public static string ToLocalFormat(this DateTimeOffset? dateTime)
        {
            if (!dateTime.HasValue)
                return string.Empty;

            return dateTime.Value.ToLocalTime().ToString("yyyy/MM/dd hh:mm tt");
        }
    }
}
