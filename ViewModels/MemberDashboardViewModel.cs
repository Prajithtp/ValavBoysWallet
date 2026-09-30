using ValavBoysWallet.Models;

namespace ValavBoysWallet.ViewModels
{
    public class MemberDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public decimal PersonalBalance { get; set; }

        public decimal GroupBalance { get; set; }

        public decimal TotalDeposited { get; set; }

        public decimal TotalWithdrawn { get; set; }

        public int PendingRequests { get; set; }

        public List<WalletTransaction> RecentTransactions { get; set; }
            = new List<WalletTransaction>();
    }
}