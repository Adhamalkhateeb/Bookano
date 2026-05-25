using ClosedXML.Excel;

namespace Bookano.Web.Extensions
{
    public static class ExcelSheetExtensions
    {
        private static int _startRow = 9;

        public static void SetHeader(
            this IXLWorksheet sheet,
            IWebHostEnvironment env,
            params string[] headers
        )
        {
            var path = Path.Combine(env.WebRootPath, "assets", "imgs", "logo.png");

            sheet.AddPicture(path).MoveTo(sheet.Cell("A1")).Scale(0.15);

            for (int i = 0; i < headers.Length; i++)
                sheet.Cell(_startRow, i + 1).SetValue(headers[i]);
        }

        public static void Format(this IXLWorksheet sheet)
        {
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.ColumnsUsed().AdjustToContents();

            sheet
                .CellsUsed()
                .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorderColor(XLColor.Black);
        }

        public static void AddTable(this IXLWorksheet sheet, int numberOfRows, int numberOfColumns)
        {
            var range = sheet.Range(_startRow, 1, _startRow + numberOfRows, numberOfColumns);
            var table = range.CreateTable();

            table.Theme = XLTableTheme.TableStyleMedium16;
            table.ShowAutoFilter = false;
            sheet.ShowGridLines = false;
        }
    }
}
