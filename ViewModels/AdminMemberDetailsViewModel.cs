namespace ValavBoysWallet.ViewModels
{
    public class AdminMemberDetailsViewModel
    {
        // =========================================
        // Member
        // =========================================

        public string UserId { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsDisabled { get; set; }


        // =========================================
        // Wallet Summary
        // =========================================

        public decimal CurrentBalance { get; set; }

        public decimal TotalDeposited { get; set; }

        public decimal TotalExpenses { get; set; }

        public int TotalTransactions { get; set; }

        public int PendingTransactions { get; set; }


        // =========================================
        // Transaction History
        // =========================================

        public List<AdminMemberTransactionViewModel> Transactions
        { get; set; } = new();
    }


    public class AdminMemberTransactionViewModel
    {
        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }
            = string.Empty;

        public string Status { get; set; }
            = string.Empty;

        public string? Category { get; set; }

        public string? Purpose { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTime? TransactionDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? RejectionReason { get; set; }
    }
}