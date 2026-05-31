using OpenHtmlToPdf;

namespace Bookano.Web.Services.PDF
{
    public class PdfService : IPdfService
    {
        private readonly IViewRendererService _viewRenderer;

        public PdfService(IViewRendererService viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public async Task<byte[]> GeneratePdfFromViewAsync<T>(ControllerContext context, string viewPath, T model, bool landscape = true)
        {
            var html = await _viewRenderer.RenderViewToStringAsync(context, viewPath, model!);

            var pdfBuilder = Pdf.From(html)
                .EncodedWith("Utf-8")
                .WithMargins(1.Centimeters());

            if (landscape)
            {
                pdfBuilder = pdfBuilder.Landscape();
            }

            return pdfBuilder.Content();
        }

    }
}
