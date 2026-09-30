namespace ValavBoysWallet.ViewModels
{
    public class MemberContributionViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public decimal TotalDeposited { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal CurrentBalance { get; set; }

        public int PendingRequests { get; set; }
    }
}