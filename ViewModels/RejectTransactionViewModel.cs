using System.ComponentModel.DataAnnotations;

namespace ValavBoysWallet.ViewModels
{
    public class RejectTransactionViewModel
    {
        [Required]
        public int TransactionId { get; set; }


        [Required(
            ErrorMessage = "Please provide a rejection reason.")]
        [StringLength(
            250,
            ErrorMessage = "Rejection reason cannot exceed 250 characters.")]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }
            = string.Empty;
    }
}