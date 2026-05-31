namespace Bookano.Application.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ReportColumnAttribute : Attribute
{
    public string Name { get; }
    public int Order { get; }

    public ReportColumnAttribute(string name, int order = 0)
    {
        Name = name;
        Order = order;
    }
}
