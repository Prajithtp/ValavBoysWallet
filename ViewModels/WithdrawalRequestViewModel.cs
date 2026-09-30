using System.ComponentModel.DataAnnotations;

namespace ValavBoysWallet.ViewModels
{
    public class WithdrawalRequestViewModel
    {
        [Required]
        [Range(1, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        [Display(Name = "Purpose")]
        public string Purpose { get; set; } = string.Empty;
    }
}