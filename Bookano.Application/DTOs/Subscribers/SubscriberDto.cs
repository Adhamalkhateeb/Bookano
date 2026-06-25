using Bookano.Domain.Enums;

namespace Bookano.Application.DTOs.Subscribers;

public sealed class SubscriberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string NationalId { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool HasWhatsApp { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string ImageThumbnailUrl { get; set; } = null!;
    public string ImagePublicId { get; set; } = null!;
    public int AreaId { get; set; }
    public string Area { get; set; } = null!;
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool IsBlackListed { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
}
