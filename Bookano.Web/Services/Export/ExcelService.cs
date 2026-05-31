using System.Reflection;
using ClosedXML.Excel;
using Bookano.Application.Attributes;

namespace Bookano.Web.Services.Export
{
    public class ExcelService : IExcelService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExcelService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName)
        {
            var columns = typeof(T).GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attr = p.GetCustomAttribute<ReportColumnAttribute>()
                })
                .Where(x => x.Attr != null)
                .OrderBy(x => x.Attr!.Order)
                .ToList();

            if (!columns.Any())
            {
                throw new InvalidOperationException($"Type '{typeof(T).Name}' must have properties decorated with [ReportColumn] to generate Excel.");
            }

            var headers = columns.Select(c => c.Attr!.Name).ToArray();
            var dataList = data.ToList();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(sheetName);

           
            ws.SetHeader(_webHostEnvironment, headers);

            int excelDataStartRow = 10;

            for (int i = 0; i < dataList.Count; i++)
            {
                var item = dataList[i];
                for (int col = 0; col < columns.Count; col++)
                {
                    var prop = columns[col].Property;
                    var val = prop.GetValue(item);
                    var cell = ws.Cell(i + excelDataStartRow, col + 1);

                    if (val == null)
                    {
                        cell.SetValue("-");
                    }
                    else if (val is IEnumerable<string> list)
                    {
                        cell.SetValue(string.Join(", ", list));
                    }
                    else if (val is DateTime dateTime)
                    {
                        cell.SetValue(dateTime.ToString("d MMM, yyyy"));
                    }
                    else if (val is DateOnly dateOnly)
                    {
                        cell.SetValue(dateOnly.ToString("d MMM, yyyy"));
                    }
                    else if (val is bool boolean)
                    {
                        cell.SetValue(boolean ? "Yes" : "No");
                    }
                    else
                    {
                        cell.SetValue(XLCellValue.FromObject(val));
                    }
                }
            }

            ws.Format();
            ws.AddTable(dataList.Count, headers.Length);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
