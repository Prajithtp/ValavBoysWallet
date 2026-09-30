using System.ComponentModel.DataAnnotations;

namespace ValavBoysWallet.ViewModels
{
    public class AdminReportViewModel
    {
        // =========================================
        // Filters
        // =========================================

        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Transaction Type")]
        public string? TransactionType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }


        // =========================================
        // Summary
        // =========================================

        public decimal GroupBalance { get; set; }

        public decimal TotalDeposits { get; set; }

        public decimal TotalExpenses { get; set; }

        public int TotalTransactions { get; set; }

        public int ApprovedTransactions { get; set; }

        public int PendingTransactions { get; set; }

        public int RejectedTransactions { get; set; }


        // =========================================
        // Report rows
        // =========================================

        public List<AdminReportTransactionViewModel> Transactions
        { get; set; } = new();
    }


    public class AdminReportTransactionViewModel
    {
        public int TransactionId { get; set; }

        public string MemberName { get; set; }
            = string.Empty;

        public string MemberEmail { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public string Type { get; set; }
            = string.Empty;

        public string? Category { get; set; }

        public string? Purpose { get; set; }

        public string? ReferenceNumber { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public DateTime? TransactionDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}