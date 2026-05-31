
namespace Bookano.Application.Common.Interfaces;

public interface IExcelService
{
    byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName);
}
