namespace ValavBoysWallet.ViewModels
{
    public class AdminTransactionViewModel
    {
        public int TransactionId { get; set; }

        public string MemberName { get; set; }
            = string.Empty;

        public string MemberEmail { get; set; }
            = string.Empty;

        public decimal Amount { get; set; }

        public string Type { get; set; }
            = string.Empty;

        public string ReferenceNumber { get; set; }
            = string.Empty;

        public string Purpose { get; set; }
            = string.Empty;

        public string? ScreenshotPath { get; set; }

        // Actual payment date entered by the member.
        // This is mainly used for deposit requests.
        public DateTime? TransactionDate { get; set; }

        // Date/time the request was submitted to the application.
        public DateTime CreatedDate { get; set; }
    }
}