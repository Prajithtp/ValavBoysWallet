using ValavBoysWallet.Models.Enums;

namespace ValavBoysWallet.ViewModels
{
    public class AdminDashboardViewModel
    {
        // -----------------------------------------
        // Wallet Summary
        // -----------------------------------------

        public decimal GroupBalance { get; set; }

        public decimal TotalApprovedDeposits { get; set; }

        public decimal TotalApprovedExpenses { get; set; }


        // -----------------------------------------
        // Member Summary
        // -----------------------------------------

        public int TotalMembers { get; set; }

        public int ActiveMembers { get; set; }

        public int DisabledMembers { get; set; }


        // -----------------------------------------
        // Transaction Summary
        // -----------------------------------------

        public int PendingTransactions { get; set; }

        public int ApprovedTransactions { get; set; }

        public int RejectedTransactions { get; set; }


        // -----------------------------------------
        // Recent Activity
        // -----------------------------------------

        public List<AdminDashboardTransactionViewModel>
            RecentTransactions
        { get; set; } = new();
    }


    public class AdminDashboardTransactionViewModel
    {
        public int TransactionId { get; set; }

        public string MemberName { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        public string? Purpose { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}