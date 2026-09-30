namespace ValavBoysWallet.ViewModels
{
    public class AdminApprovalPanelViewModel
    {
        // -------------------------------------------------
        // Summary Cards
        // -------------------------------------------------

        public int PendingRequests { get; set; }

        public int ApprovedRequests { get; set; }

        public int RejectedRequests { get; set; }

        public int TotalProcessed { get; set; }


        // -------------------------------------------------
        // Filter Values
        // -------------------------------------------------

        public string? TransactionType { get; set; }

        public string? MemberId { get; set; }

        public string? DateFilter { get; set; }

        public string? SearchTerm { get; set; }


        // -------------------------------------------------
        // Member Filter Options
        // -------------------------------------------------

        public List<AdminApprovalMemberViewModel> Members
        { get; set; } = new();


        // -------------------------------------------------
        // Pending Transactions
        // -------------------------------------------------

        public List<AdminTransactionViewModel> PendingTransactions
        { get; set; } = new();
    }


    public class AdminApprovalMemberViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
    }
}