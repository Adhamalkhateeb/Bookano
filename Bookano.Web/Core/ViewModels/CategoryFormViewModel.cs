using Bookano.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Bookano.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100,ErrorMessage = "Name can't exceed 100 characters.")]
        [Remote("AllowItem",null,ErrorMessage ="Category already exists.",AdditionalFields = nameof(Id))]
        public string Name { get; set; } = null!;
    }
}
