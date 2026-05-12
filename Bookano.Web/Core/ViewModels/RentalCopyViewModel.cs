namespace Bookano.Web.Core.ViewModels
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }

        public DateOnly RentalDate { get; set; }

        public DateOnly EndDate { get; set; }

        public DateOnly? ReturnDate { get; set; }

        public DateOnly? ExtendedOn { get; set; }

        public int DelayInDays
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var dueDate = ExtendedOn ?? EndDate;

                if (ReturnDate.HasValue && ReturnDate.Value > dueDate)
                {
                    return (
                        ReturnDate.Value.ToDateTime(TimeOnly.MinValue)
                        - dueDate.ToDateTime(TimeOnly.MinValue)
                    ).Days;
                }

                if (!ReturnDate.HasValue && today > dueDate)
                {
                    return (
                        today.ToDateTime(TimeOnly.MinValue)
                        - dueDate.ToDateTime(TimeOnly.MinValue)
                    ).Days;
                }

                return 0;
            }
        }
    }
}
