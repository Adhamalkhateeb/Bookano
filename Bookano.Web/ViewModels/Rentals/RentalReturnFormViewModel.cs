using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.ViewModels.Rentals
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Penality Paid?")]
        [AssertThat(
            "TotalDelayInDays == 0 || PenalityPaid",
            ErrorMessage = Error.PenalityShouldBePaid
        )]
        public bool PenalityPaid { get; set; }
        public IList<RentalCopyViewModel> RentalCopies { get; set; } = [];

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays { get; set; }
    }
}
