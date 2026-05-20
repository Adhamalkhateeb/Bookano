namespace Bookano.Web.Services.PDF
{
    public interface IViewRendererService
    {
        Task<string> RenderViewToStringAsync(ControllerContext actionContext, string viewPath, object model);
    }
}
