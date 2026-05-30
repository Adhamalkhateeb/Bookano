namespace Bookano.Application.DTOs.Subscribers;

public sealed class SubscriberFormDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string NationalId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public bool HasWhatsApp { get; set; }
    public int GovernorateId { get; set; }
    public int AreaId { get; set; }
    public string Address { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string ImageThumbnailUrl { get; set; } = null!;
    public string ImagePublicId { get; set; } = null!;
    public ImageUploadDto? Image { get; set; }
}
