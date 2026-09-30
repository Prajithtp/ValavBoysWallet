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
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WalletController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var wallet = await _context.GroupWallets
                .FirstOrDefaultAsync();

            var transactions = await _context.WalletTransactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var lastResetAt = wallet?.LastResetAt;

            var cycleTransactions = transactions
                .Where(x =>
                    !lastResetAt.HasValue ||
                    x.ApprovedDate > lastResetAt.Value)
                .ToList();

            var model = new MemberDashboardViewModel
            {
                FullName = user.FullName,

                PersonalBalance = user.CurrentBalance,

                GroupBalance = wallet?.TotalBalance ?? 0,

                TotalDeposited = transactions
                    .Where(x =>
                        x.Status == TransactionStatus.Approved &&
                        x.Type == TransactionType.Deposit)
                    .Sum(x => x.Amount),

                TotalWithdrawn = transactions
                    .Where(x =>
                        x.Status == TransactionStatus.Approved &&
                        x.Type == TransactionType.Withdrawal)
                    .Sum(x => x.Amount),

                PendingRequests = transactions.Count(x =>
                    x.Status == TransactionStatus.Pending),

                RecentTransactions = transactions
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }
    }
}