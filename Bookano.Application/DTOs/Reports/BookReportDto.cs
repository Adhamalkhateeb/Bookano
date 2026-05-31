using Bookano.Application.Attributes;

namespace Bookano.Application.DTOs.Reports;

public sealed class BookReportDto
{
    [ReportColumn("ISBN", 1)]
    public string Isbn { get; set; } = null!;

    [ReportColumn("Title", 2)]
    public string Title { get; set; } = null!;

    [ReportColumn("Authors", 3)]
    public IEnumerable<string> Authors { get; set; } = [];

    [ReportColumn("Categories", 4)]
    public IEnumerable<string> Categories { get; set; } = [];

    [ReportColumn("Publisher", 5)]
    public string Publisher { get; set; } = null!;

    [ReportColumn("Publishing Date", 6)]
    public DateOnly PublishingDate { get; set; }

    [ReportColumn("Hall", 7)]
    public string Hall { get; set; } = null!;

    public bool IsAvailableForRental { get; set; }
    public bool IsDeleted { get; set; }

    [ReportColumn("Available For Rental", 8)]
    public string AvailableForRentalText => IsAvailableForRental ? "Yes" : "No";

    [ReportColumn("Status", 9)]
    public string StatusText => IsDeleted ? "Deleted" : "Active";
}
