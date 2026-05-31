using Bookano.Web.ViewModels.BookCopies;

namespace Bookano.Web.ViewModels.Rentals
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }

        public DateTime RentalDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public DateTime? ExtendedOn { get; set; }

        public bool? IsReturned { get; set; }

        public int DelayInDays
        {
            get
            {
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    return (ReturnDate.Value.Subtract(EndDate)).Days;

                if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                    return (DateTime.Today.Subtract(EndDate)).Days;

                return 0;
            }
        }
    }
}
