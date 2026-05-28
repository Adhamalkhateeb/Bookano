namespace Bookano.Domain.Entities;

public sealed class Rental : BaseEntity
{
    public int Id { get; set; }

    public int SubscriberId { get; set; }
    public Subscriber? Subscriber { get; set; }

    public DateOnly StartDate { get; set; }

    public bool PenaltyPaid { get; set; }

    public ICollection<RentalCopy> RentalCopies { get; set; } = [];
}
