using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ValavBoysWallet.Models.Enums;

namespace ValavBoysWallet.Models
{
    public class WalletTransaction
    {
        [Key]
        public int TransactionId { get; set; }


        // --------------------------------
        // Member
        // --------------------------------

        [Required]
        public string UserId { get; set; }
            = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }


        // --------------------------------
        // Transaction details
        // --------------------------------

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(250)]
        public string? Purpose { get; set; }


        // --------------------------------
        // Actual transaction date
        // --------------------------------

        public DateTime? TransactionDate { get; set; }


        // --------------------------------
        // Deposit proof
        // --------------------------------

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? ScreenshotPath { get; set; }


        // --------------------------------
        // Approval
        // --------------------------------

        [Required]
        public TransactionStatus Status { get; set; }
            = TransactionStatus.Pending;

        public DateTime CreatedDate { get; set; }
            = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }

        public string? ApprovedByUserId { get; set; }

        [ForeignKey(nameof(ApprovedByUserId))]
        public ApplicationUser? ApprovedByUser { get; set; }

        [StringLength(250)]
        public string? RejectionReason { get; set; }

        // --------------------------------
        // Concurrency protection
        // --------------------------------

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}