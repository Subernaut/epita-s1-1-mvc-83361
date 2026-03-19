using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels
{
    public class CreateLoanViewModel
    {
        [Required]
        [Display(Name = "Loan Date")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Required]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        public List<SelectListItem> Members { get; set; } = new();
        public List<SelectListItem> Books { get; set; } = new();
    }
}