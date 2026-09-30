namespace ValavBoysWallet.ViewModels
{
    public class AdminUserManagementViewModel
    {
        // =========================================
        // Summary
        // =========================================

        public int TotalMembers { get; set; }

        public int ActiveMembers { get; set; }

        public int DisabledMembers { get; set; }

        public decimal TotalMemberBalance { get; set; }


        // =========================================
        // Members
        // =========================================

        public List<AdminUserItemViewModel> Members
        { get; set; } = new();
    }


    public class AdminUserItemViewModel
    {
        public string UserId { get; set; }
            = string.Empty;

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string? PhoneNumber { get; set; }

        public decimal CurrentBalance { get; set; }

        public bool IsDisabled { get; set; }

        public int TotalTransactions { get; set; }

        public int PendingTransactions { get; set; }

        public decimal TotalDeposited { get; set; }

        public decimal TotalExpenses { get; set; }
    }
}