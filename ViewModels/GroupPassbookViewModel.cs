namespace ValavBoysWallet.ViewModels
{
    public class GroupPassbookViewModel
    {
        public decimal GroupBalance { get; set; }

        public decimal TotalDeposits { get; set; }

        public decimal TotalExpenses { get; set; }

        public List<PassbookTransactionViewModel> Transactions { get; set; }
            = new List<PassbookTransactionViewModel>();
    }


    public class PassbookTransactionViewModel
    {
        public int TransactionId { get; set; }

        public string MemberName { get; set; }
            = string.Empty;

        public string MemberEmail { get; set; }
            = string.Empty;

        public string Type { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public string Category { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;


        // Actual payment/transaction date.
        // Primarily used for deposits.
        public DateTime? ActualTransactionDate { get; set; }


        // Date/time the transaction affected
        // the shared wallet balance.
        public DateTime PostedDate { get; set; }


        public decimal RunningBalance { get; set; }
    }
}