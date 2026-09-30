using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ValavBoysWallet.ViewModels
{
    public class DepositRequestViewModel
    {
        [Required]
        [Range(
            1,
            1000000,
            ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }


        [Required]
        [StringLength(100)]
        [Display(Name = "UPI / Bank Reference Number")]
        public string ReferenceNumber { get; set; }
            = string.Empty;


        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; }
            = DateTime.Today;


        [Required(
            ErrorMessage = "Please upload the payment screenshot.")]
        [Display(Name = "Payment Screenshot")]
        public IFormFile? Screenshot { get; set; }
    }
}