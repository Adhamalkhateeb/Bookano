using Bookano.Web.ViewModels.Rentals;

namespace Bookano.Web.ViewModels.Subscribers
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Address { get; set; }
        public bool IsBlackListed { get; set; }
        public DateTimeOffset CreatedOnUtc { get; set; }
        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; } = [];
        public IEnumerable<RentalViewModel> Rentals { get; set; } = [];

        public DateTime? LastSubscriptionEndDate
        {
            get
            {
                if (!Subscriptions.Any())
                    return null;

                return Subscriptions.Max(subscription => subscription.EndDate);
            }
        }

        public int TotalRentalCopies => Rentals.Sum(rental => rental.NumberOfCopies);

        public int ActiveRentalCopies => Rentals.Sum(rental => rental.ActiveCopies);

        public SubscriberStatus Status { get; set; }

        public string StatusClass
        {
            get
            {
                return Status switch
                {
                    SubscriberStatus.Active => "success",
                    SubscriberStatus.Banned => "danger",
                    _ => "warning",
                };
            }
        }

        public string StatusIcon
        {
            get
            {
                return Status switch
                {
                    SubscriberStatus.Active => "fa-award",
                    SubscriberStatus.Banned => "fa-ban",
                    _ => "fa-triangle-exclamation",
                };
            }
        }

        public bool CanAddRental { get; set; }
    }
}
