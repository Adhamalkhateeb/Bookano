namespace Bookano.Web.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        [Range(1, 1000, ErrorMessage = Error.ShouldBeInRange), Display(Name = "Edition Number")]
        public int EditionNumber { get; set; }

        [Display(Name = "Is available for rental?")]
        public bool IsAvailableForRental { get; set; }
        public bool ShowRentalInput { get; set; }
    }
}
