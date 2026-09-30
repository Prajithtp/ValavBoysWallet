using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Data;
using ValavBoysWallet.Models;
using ValavBoysWallet.Models.Enums;
using ValavBoysWallet.ViewModels;

namespace ValavBoysWallet.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // -------------------------------------------------
        // Member Contribution Overview
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(x => x.FullName)
                .ToListAsync();


            var transactions = await _context.WalletTransactions
                .ToListAsync();

            var lastResetAt = await _context.GroupWallets
            .AsNoTracking()
            .Select(x => x.LastResetAt)
            .FirstOrDefaultAsync();
            var members =
                new List<MemberContributionViewModel>();


            foreach (var user in users)
            {
                // Do not show Admin account
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    continue;
                }


                var userTransactions = transactions
                    .Where(x => x.UserId == user.Id)
                    .ToList();

                var cycleTransactions = userTransactions
               .Where(x =>
               !lastResetAt.HasValue ||
               x.ApprovedDate > lastResetAt.Value)
               .ToList();

                var totalDeposited = cycleTransactions
                    .Where(x =>
                        x.Type == TransactionType.Deposit &&
                        x.Status == TransactionStatus.Approved)
                    .Sum(x => x.Amount);


                var totalExpenses = cycleTransactions
                     .Where(x =>
                        x.Type == TransactionType.Withdrawal &&
                        x.Status == TransactionStatus.Approved)
                    .Sum(x => x.Amount);


                var pendingRequests = userTransactions
                    .Count(x =>
                        x.Status == TransactionStatus.Pending);


                members.Add(
                    new MemberContributionViewModel
                    {
                        UserId = user.Id,

                        FullName = user.FullName,

                        Email = user.Email ?? string.Empty,

                        TotalDeposited = totalDeposited,

                        TotalExpenses = totalExpenses,

                        CurrentBalance = user.CurrentBalance,

                        PendingRequests = pendingRequests
                    });
            }


            return View(members);
        }
    }
}