namespace Bookano.Application.DTOs.Reports;

public sealed class RentalsReportDto
{
    public int SubscriberId { get; set; }
    public string SubscriberName { get; set; } = null!;
    public string SubscriberMobile { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public int BookSerialNumber { get; set; }
    public IEnumerable<string> BookAuthors { get; set; } = [];
    public DateOnly RentalDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public DateOnly? ExtendedOn { get; set; }
}
