using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookano.Web.Services.PDF
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfFromViewAsync<T>(ControllerContext context, string viewPath, T model, bool landscape = true);
    }
}
