namespace Bookano.Application.Common.Models;

public class ValidationError
{
    public string PropertyName { get; }
    public string Error { get; }

    public ValidationError(string propertyName, string error)
    {
        PropertyName = propertyName;
        Error = error;
    }
}
