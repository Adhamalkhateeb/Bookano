using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookano.Web.Core.ViewModels
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
        public IList<ReturnCopyViewModel> SelectedCopies { get; set; } = [];

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays => RentalCopies.Sum(c => c.DelayInDays);
    }
}
