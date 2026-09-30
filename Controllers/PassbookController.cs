using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Data;
using ValavBoysWallet.Models.Enums;
using ValavBoysWallet.ViewModels;

namespace ValavBoysWallet.Controllers
{
    [Authorize]
    public class PassbookController : Controller
    {
        private readonly ApplicationDbContext _context;


        public PassbookController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // -------------------------------------------------
        // Group Passbook
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ---------------------------------------------
            // Get approved transactions in POSTED order.
            //
            // Running balance must follow the order in
            // which transactions actually affected
            // the wallet.
            // ---------------------------------------------

            var approvedTransactions =
                await _context.WalletTransactions
                    .Include(x => x.User)
                    .Where(x =>
                        x.Status ==
                        TransactionStatus.Approved)
                    .OrderBy(x =>
                        x.ApprovedDate ??
                        x.CreatedDate)
                    .ThenBy(x =>
                        x.TransactionId)
                    .ToListAsync();


            decimal runningBalance = 0;


            var passbookItems =
                new List<PassbookTransactionViewModel>();


            // ---------------------------------------------
            // Build running balance
            // ---------------------------------------------

            foreach (var transaction in
                     approvedTransactions)
            {
                if (transaction.Type ==
                    TransactionType.Deposit)
                {
                    runningBalance +=
                        transaction.Amount;
                }
                else
                {
                    runningBalance -=
                        transaction.Amount;
                }


                // -----------------------------------------
                // Description
                // -----------------------------------------

                var description =
                    transaction.Type ==
                    TransactionType.Deposit

                        ? transaction.ReferenceNumber
                          ?? "-"

                        : transaction.Purpose
                          ?? "-";


                // -----------------------------------------
                // Posted Date
                //
                // ApprovedDate is when the balance
                // actually changed.
                //
                // CreatedDate is a fallback for older
                // records if ApprovedDate is missing.
                // -----------------------------------------

                var postedDate =
                    transaction.ApprovedDate
                    ?? transaction.CreatedDate;


                // -----------------------------------------
                // Add passbook row
                // -----------------------------------------

                passbookItems.Add(
                    new PassbookTransactionViewModel
                    {
                        TransactionId =
                            transaction.TransactionId,


                        MemberName =
                            transaction.User?.FullName
                            ?? "Unknown Member",


                        MemberEmail =
                            transaction.User?.Email
                            ?? string.Empty,


                        Type =
                            transaction.Type.ToString(),


                        Amount =
                            transaction.Amount,


                        Category =
                            transaction.Category
                            ?? "-",


                        Description =
                            description,


                        // Actual payment date entered
                        // by the member.
                        //
                        // Older deposits may be null.
                        ActualTransactionDate =
                            transaction.Type ==
                            TransactionType.Deposit

                                ? transaction.TransactionDate

                                : null,


                        // Date/time the wallet balance
                        // actually changed.
                        PostedDate =
                            postedDate,


                        RunningBalance =
                            runningBalance
                    });
            }


            // ---------------------------------------------
            // Current wallet
            // ---------------------------------------------

            var wallet =
                await _context.GroupWallets
                    .FirstOrDefaultAsync();


            // ---------------------------------------------
            // Page ViewModel
            // ---------------------------------------------

            var model =
                new GroupPassbookViewModel
                {
                    GroupBalance =
                        wallet?.TotalBalance ?? 0,


                    TotalDeposits =
                        approvedTransactions
                            .Where(x =>
                                x.Type ==
                                TransactionType.Deposit)
                            .Sum(x =>
                                x.Amount),


                    TotalExpenses =
                        approvedTransactions
                            .Where(x =>
                                x.Type ==
                                TransactionType.Withdrawal)
                            .Sum(x =>
                                x.Amount),


                    // Display newest POSTED transaction
                    // first.
                    //
                    // RunningBalance was already
                    // calculated oldest -> newest.
                    Transactions =
                        passbookItems
                            .OrderByDescending(x =>
                                x.PostedDate)
                            .ThenByDescending(x =>
                                x.TransactionId)
                            .ToList()
                };


            return View(model);
        }
    }
}