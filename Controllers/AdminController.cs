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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -------------------------------------------------
        // Admin Dashboard
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // ---------------------------------------------
            // Current Group Balance
            // ---------------------------------------------

            var groupBalance =
                await _context.GroupWallets
                    .AsNoTracking()
                    .Select(x => x.TotalBalance)
                    .FirstOrDefaultAsync();


            // ---------------------------------------------
            // Member Accounts
            // ---------------------------------------------

            var memberUsers =
                await _userManager
                    .GetUsersInRoleAsync("Member");

            var totalMembers =
                memberUsers.Count;

            var disabledMembers =
                memberUsers.Count(user =>
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value >
                        DateTimeOffset.UtcNow);

            var activeMembers =
                totalMembers - disabledMembers;


            // ---------------------------------------------
            // Transaction Summary
            // ---------------------------------------------

            var lastResetAt =
                await _context.GroupWallets
                    .AsNoTracking()
                    .Select(x => x.LastResetAt)
                    .FirstOrDefaultAsync();

            var approvedTransactionsForTotals =
                _context.WalletTransactions
                    .AsNoTracking()
                    .Where(x => x.Status == TransactionStatus.Approved);

            if (lastResetAt.HasValue)
            {
                approvedTransactionsForTotals =
                    approvedTransactionsForTotals.Where(x =>
                        x.ApprovedDate > lastResetAt.Value);
            }

            var totalApprovedDeposits =
                await approvedTransactionsForTotals
                    .Where(x => x.Type == TransactionType.Deposit)
                    .SumAsync(x => (decimal?)x.Amount)
                ?? 0;

            var totalApprovedExpenses =
                await approvedTransactionsForTotals
                    .Where(x => x.Type == TransactionType.Withdrawal)
                    .SumAsync(x => (decimal?)x.Amount)
                ?? 0;


            var pendingTransactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Pending);

            var approvedTransactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Approved);

            var rejectedTransactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Rejected);


            // ---------------------------------------------
            // Recent Activity
            // ---------------------------------------------

            var recentTransactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .Include(x => x.User)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(8)
                    .Select(x =>
                        new AdminDashboardTransactionViewModel
                        {
                            TransactionId =
                                x.TransactionId,

                            MemberName =
                                x.User != null
                                    ? x.User.FullName
                                    : "Unknown",

                            Amount =
                                x.Amount,

                            Type =
                                x.Type,

                            Status =
                                x.Status,

                            Purpose =
                                x.Purpose,

                            ReferenceNumber =
                                x.ReferenceNumber,

                            CreatedDate =
                                x.CreatedDate
                        })
                    .ToListAsync();


            // ---------------------------------------------
            // Build Dashboard ViewModel
            // ---------------------------------------------

            var model =
                new AdminDashboardViewModel
                {
                    GroupBalance =
                        groupBalance,

                    TotalApprovedDeposits =
                        totalApprovedDeposits,

                    TotalApprovedExpenses =
                        totalApprovedExpenses,

                    TotalMembers =
                        totalMembers,

                    ActiveMembers =
                        activeMembers,

                    DisabledMembers =
                        disabledMembers,

                    PendingTransactions =
                        pendingTransactions,

                    ApprovedTransactions =
                        approvedTransactions,

                    RejectedTransactions =
                        rejectedTransactions,

                    RecentTransactions =
                        recentTransactions
                };


            return View(model);
        }

        // -------------------------------------------------
        // Pending Approval Panel
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Index(
    string? transactionType,
    string? memberId,
    string? dateFilter,
    string? searchTerm)
        {
            // ---------------------------------------------
            // Base pending transaction query
            // ---------------------------------------------

            var query =
                _context.WalletTransactions
                    .AsNoTracking()
                    .Include(x => x.User)
                    .Where(x =>
                        x.Status == TransactionStatus.Pending)
                    .AsQueryable();


            // ---------------------------------------------
            // Transaction Type Filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(transactionType) &&
                Enum.TryParse<TransactionType>(
                    transactionType,
                    true,
                    out var parsedType))
            {
                query = query.Where(x =>
                    x.Type == parsedType);
            }


            // ---------------------------------------------
            // Member Filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(memberId))
            {
                query = query.Where(x =>
                    x.UserId == memberId);
            }


            // ---------------------------------------------
            // Date Filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                var today =
                    DateTime.UtcNow.Date;

                switch (dateFilter)
                {
                    case "Today":
                        query = query.Where(x =>
                            x.CreatedDate >= today);
                        break;

                    case "Last7Days":
                        var last7Days =
                            today.AddDays(-6);

                        query = query.Where(x =>
                            x.CreatedDate >= last7Days);
                        break;

                    case "Last30Days":
                        var last30Days =
                            today.AddDays(-29);

                        query = query.Where(x =>
                            x.CreatedDate >= last30Days);
                        break;
                }
            }


            // ---------------------------------------------
            // Search Filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch =
                    searchTerm.Trim();

                query = query.Where(x =>
                    (x.User != null &&
                     x.User.FullName.Contains(normalizedSearch)) ||

                    (x.ReferenceNumber != null &&
                     x.ReferenceNumber.Contains(normalizedSearch)) ||

                    (x.Purpose != null &&
                     x.Purpose.Contains(normalizedSearch)));
            }


            // ---------------------------------------------
            // Get Filtered Pending Transactions
            // ---------------------------------------------

            var pendingTransactions =
                await query
                    .OrderBy(x => x.CreatedDate)
                    .Select(x =>
                        new AdminTransactionViewModel
                        {
                            TransactionId =
                                x.TransactionId,

                            MemberName =
                                x.User != null
                                    ? x.User.FullName
                                    : "Unknown",

                            MemberEmail =
                                x.User != null
                                    ? x.User.Email ?? string.Empty
                                    : string.Empty,

                            Amount =
                                x.Amount,

                            Type =
                                x.Type.ToString(),

                            ReferenceNumber =
                                x.ReferenceNumber ?? "-",

                            Purpose =
                                x.Purpose ?? "-",

                            ScreenshotPath =
                                x.ScreenshotPath,

                            TransactionDate =
                                x.TransactionDate,

                            CreatedDate =
                                x.CreatedDate
                        })
                    .ToListAsync();


            // ---------------------------------------------
            // Overall Approval Statistics
            // ---------------------------------------------

            var totalPendingRequests =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Pending);

            var approvedRequests =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Approved);

            var rejectedRequests =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.Status == TransactionStatus.Rejected);


            // ---------------------------------------------
            // Member Dropdown Options
            // ---------------------------------------------

            var memberUsers =
                await _userManager
                    .GetUsersInRoleAsync("Member");

            var members =
                memberUsers
                    .OrderBy(x => x.FullName)
                    .Select(x =>
                        new AdminApprovalMemberViewModel
                        {
                            UserId =
                                x.Id,

                            FullName =
                                x.FullName
                        })
                    .ToList();


            // ---------------------------------------------
            // Build ViewModel
            // ---------------------------------------------

            var model =
                new AdminApprovalPanelViewModel
                {
                    PendingRequests =
                        totalPendingRequests,

                    ApprovedRequests =
                        approvedRequests,

                    RejectedRequests =
                        rejectedRequests,

                    TotalProcessed =
                        approvedRequests +
                        rejectedRequests,

                    TransactionType =
                        transactionType,

                    MemberId =
                        memberId,

                    DateFilter =
                        dateFilter,

                    SearchTerm =
                        searchTerm,

                    Members =
                        members,

                    PendingTransactions =
                        pendingTransactions
                };


            return View(model);
        }

        // -------------------------------------------------
        // Approve
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var adminUserId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -----------------------------------------
                // Find transaction
                // -----------------------------------------

                var transaction =
                    await _context.WalletTransactions
                        .Include(x => x.User)
                        .FirstOrDefaultAsync(
                            x => x.TransactionId == id);

                if (transaction == null)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "Transaction not found.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Must still be pending
                // -----------------------------------------

                if (transaction.Status !=
                    TransactionStatus.Pending)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "This transaction has already been processed.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Member must exist
                // -----------------------------------------

                if (transaction.User == null)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "Member account could not be found.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Find group wallet
                // -----------------------------------------

                var wallet =
                    await _context.GroupWallets
                        .FirstOrDefaultAsync();

                if (wallet == null)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "Group wallet could not be found.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Deposit Approval
                // -----------------------------------------

                if (transaction.Type ==
                    TransactionType.Deposit)
                {
                    wallet.TotalBalance +=
                        transaction.Amount;

                    transaction.User.CurrentBalance +=
                        transaction.Amount;
                }


                // -----------------------------------------
                // Withdrawal Approval
                // -----------------------------------------

                else if (transaction.Type ==
                         TransactionType.Withdrawal)
                {
                    if (wallet.TotalBalance <
                        transaction.Amount)
                    {
                        await dbTransaction.RollbackAsync();

                        TempData["ErrorMessage"] =
                            "Group wallet does not have enough balance.";

                        return RedirectToAction(nameof(Index));
                    }

                    if (transaction.User.CurrentBalance <
                        transaction.Amount)
                    {
                        await dbTransaction.RollbackAsync();

                        TempData["ErrorMessage"] =
                            "Member does not have enough balance.";

                        return RedirectToAction(nameof(Index));
                    }

                    wallet.TotalBalance -=
                        transaction.Amount;

                    transaction.User.CurrentBalance -=
                        transaction.Amount;
                }
                else
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "Invalid transaction type.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Mark transaction as approved
                // -----------------------------------------

                transaction.Status =
                    TransactionStatus.Approved;

                transaction.ApprovedDate =
                    DateTime.UtcNow;

                transaction.ApprovedByUserId =
                    adminUserId;

                wallet.UpdatedAt =
                    DateTime.UtcNow;


                // -----------------------------------------
                // Save
                //
                // WalletTransaction has a RowVersion.
                // EF Core includes the original RowVersion
                // when updating this transaction.
                // -----------------------------------------

                await _context.SaveChangesAsync();


                // -----------------------------------------
                // Commit only after SaveChanges succeeds
                // -----------------------------------------

                await dbTransaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Transaction approved successfully.";
            }


            // ---------------------------------------------
            // Concurrency conflict
            //
            // Another request changed the transaction
            // after this request loaded it.
            // ---------------------------------------------

            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();

                _context.ChangeTracker.Clear();

                TempData["ErrorMessage"] =
                    "This transaction was already processed by another request. " +
                    "No duplicate balance update was applied.";
            }


            // ---------------------------------------------
            // Other unexpected error
            // ---------------------------------------------

            catch
            {
                await dbTransaction.RollbackAsync();

                _context.ChangeTracker.Clear();

                TempData["ErrorMessage"] =
                    "Something went wrong while approving the transaction.";
            }


            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------
        // Reject
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            RejectTransactionViewModel model)
        {
            var adminUserId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }


            // ---------------------------------------------
            // Validate rejection reason
            // ---------------------------------------------

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please provide a valid rejection reason.";

                return RedirectToAction(nameof(Index));
            }

            var rejectionReason =
                model.RejectionReason.Trim();

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] =
                    "Rejection reason is required.";

                return RedirectToAction(nameof(Index));
            }


            try
            {
                // -----------------------------------------
                // Find transaction
                // -----------------------------------------

                var transaction =
                    await _context.WalletTransactions
                        .FirstOrDefaultAsync(x =>
                            x.TransactionId ==
                            model.TransactionId);

                if (transaction == null)
                {
                    TempData["ErrorMessage"] =
                        "Transaction not found.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Must still be pending
                // -----------------------------------------

                if (transaction.Status !=
                    TransactionStatus.Pending)
                {
                    TempData["ErrorMessage"] =
                        "This transaction has already been processed.";

                    return RedirectToAction(nameof(Index));
                }


                // -----------------------------------------
                // Reject transaction
                // -----------------------------------------

                transaction.Status =
                    TransactionStatus.Rejected;

                transaction.RejectionReason =
                    rejectionReason;

                transaction.ApprovedDate =
                    DateTime.UtcNow;

                transaction.ApprovedByUserId =
                    adminUserId;


                // -----------------------------------------
                // Save
                //
                // RowVersion protects this update.
                // If another request already approved
                // or rejected this transaction,
                // EF Core will detect the conflict.
                // -----------------------------------------

                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    "Transaction rejected successfully.";
            }


            // ---------------------------------------------
            // Concurrency conflict
            // ---------------------------------------------

            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();

                TempData["ErrorMessage"] =
                    "This transaction was already processed by another request.";
            }


            // ---------------------------------------------
            // Other unexpected error
            // ---------------------------------------------

            catch
            {
                _context.ChangeTracker.Clear();

                TempData["ErrorMessage"] =
                    "Something went wrong while rejecting the transaction.";
            }


            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------
        // Reports
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Reports(
            DateTime? fromDate,
            DateTime? toDate,
            string? transactionType,
            string? status)
        {
            // ---------------------------------------------
            // Validate date range
            // ---------------------------------------------

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                TempData["ErrorMessage"] =
                    "From Date cannot be later than To Date.";

                return RedirectToAction(nameof(Reports));
            }


            // ---------------------------------------------
            // Start transaction query
            // ---------------------------------------------

            var query =
                _context.WalletTransactions
                    .AsNoTracking()
                    .Include(x => x.User)
                    .AsQueryable();


            // ---------------------------------------------
            // From Date filter
            //
            // We use CreatedDate here so every request,
            // including withdrawals, can be filtered.
            // ---------------------------------------------

            if (fromDate.HasValue)
            {
                var startDate =
                    fromDate.Value.Date;

                query = query.Where(x =>
                    x.CreatedDate >= startDate);
            }


            // ---------------------------------------------
            // To Date filter
            //
            // Using the next day with "<" includes the
            // entire selected To Date.
            // ---------------------------------------------

            if (toDate.HasValue)
            {
                var endDateExclusive =
                    toDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.CreatedDate < endDateExclusive);
            }


            // ---------------------------------------------
            // Transaction Type filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(transactionType) &&
                Enum.TryParse<TransactionType>(
                    transactionType,
                    true,
                    out var parsedType))
            {
                query = query.Where(x =>
                    x.Type == parsedType);
            }


            // ---------------------------------------------
            // Status filter
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TransactionStatus>(
                    status,
                    true,
                    out var parsedStatus))
            {
                query = query.Where(x =>
                    x.Status == parsedStatus);
            }


            // ---------------------------------------------
            // Get filtered transactions
            // ---------------------------------------------

            var transactions =
                await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x =>
                        new AdminReportTransactionViewModel
                        {
                            TransactionId =
                                x.TransactionId,

                            MemberName =
                                x.User != null
                                    ? x.User.FullName
                                    : "Unknown",

                            MemberEmail =
                                x.User != null
                                    ? x.User.Email ?? string.Empty
                                    : string.Empty,

                            Amount =
                                x.Amount,

                            Type =
                                x.Type.ToString(),

                            Category =
                                x.Category,

                            Purpose =
                                x.Purpose,

                            ReferenceNumber =
                                x.ReferenceNumber,

                            Status =
                                x.Status.ToString(),

                            TransactionDate =
                                x.TransactionDate,

                            CreatedDate =
                                x.CreatedDate,

                            ApprovedDate =
                                x.ApprovedDate,

                            RejectionReason =
                                x.RejectionReason
                        })
                    .ToListAsync();


            // ---------------------------------------------
            // Current Group Wallet Balance
            // ---------------------------------------------

            var groupBalance =
                await _context.GroupWallets
                    .AsNoTracking()
                    .Select(x => x.TotalBalance)
                    .FirstOrDefaultAsync();


            // ---------------------------------------------
            // Financial summary
            //
            // Only APPROVED transactions count as money
            // that actually entered or left the wallet.
            // ---------------------------------------------

            var totalDeposits =
                transactions
                    .Where(x =>
                        x.Status == TransactionStatus.Approved.ToString() &&
                        x.Type == TransactionType.Deposit.ToString())
                    .Sum(x => x.Amount);

            var totalExpenses =
                transactions
                    .Where(x =>
                        x.Status == TransactionStatus.Approved.ToString() &&
                        x.Type == TransactionType.Withdrawal.ToString())
                    .Sum(x => x.Amount);


            // ---------------------------------------------
            // Build report ViewModel
            // ---------------------------------------------

            var model =
                new AdminReportViewModel
                {
                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    TransactionType =
                        transactionType,

                    Status =
                        status,

                    GroupBalance =
                        groupBalance,

                    TotalDeposits =
                        totalDeposits,

                    TotalExpenses =
                        totalExpenses,

                    TotalTransactions =
                        transactions.Count,

                    ApprovedTransactions =
                        transactions.Count(x =>
                            x.Status ==
                            TransactionStatus.Approved.ToString()),

                    PendingTransactions =
                        transactions.Count(x =>
                            x.Status ==
                            TransactionStatus.Pending.ToString()),

                    RejectedTransactions =
                        transactions.Count(x =>
                            x.Status ==
                            TransactionStatus.Rejected.ToString()),

                    Transactions =
                        transactions
                };


            return View(model);
        }

        // -------------------------------------------------
        // User Management
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            // ---------------------------------------------
            // Get users who belong to the Member role
            // ---------------------------------------------

            var memberUsers =
                await _userManager
                    .GetUsersInRoleAsync("Member");


            // ---------------------------------------------
            // Build member information
            // ---------------------------------------------

            var members =
                new List<AdminUserItemViewModel>();


            foreach (var user in memberUsers)
            {
                // -----------------------------------------
                // Get this member's transaction statistics
                // -----------------------------------------

                var transactions =
                    await _context.WalletTransactions
                        .AsNoTracking()
                        .Where(x => x.UserId == user.Id)
                        .Select(x => new
                        {
                            x.Amount,
                            x.Type,
                            x.Status
                        })
                        .ToListAsync();


                var totalDeposited =
                    transactions
                        .Where(x =>
                            x.Status == TransactionStatus.Approved &&
                            x.Type == TransactionType.Deposit)
                        .Sum(x => x.Amount);


                var totalExpenses =
                    transactions
                        .Where(x =>
                            x.Status == TransactionStatus.Approved &&
                            x.Type == TransactionType.Withdrawal)
                        .Sum(x => x.Amount);


                var pendingTransactions =
                    transactions.Count(x =>
                        x.Status == TransactionStatus.Pending);


                // -----------------------------------------
                // Identity lockout state
                // -----------------------------------------

                var isDisabled =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value > DateTimeOffset.UtcNow;


                // -----------------------------------------
                // Add member to ViewModel list
                // -----------------------------------------

                members.Add(
                    new AdminUserItemViewModel
                    {
                        UserId =
                            user.Id,

                        FullName =
                            user.FullName,

                        Email =
                            user.Email ?? string.Empty,

                        PhoneNumber =
                            user.PhoneNumber,

                        CurrentBalance =
                            user.CurrentBalance,

                        IsDisabled =
                            isDisabled,

                        TotalTransactions =
                            transactions.Count,

                        PendingTransactions =
                            pendingTransactions,

                        TotalDeposited =
                            totalDeposited,

                        TotalExpenses =
                            totalExpenses
                    });
            }


            // ---------------------------------------------
            // Sort members alphabetically
            // ---------------------------------------------

            members =
                members
                    .OrderBy(x => x.FullName)
                    .ToList();


            // ---------------------------------------------
            // Build page ViewModel
            // ---------------------------------------------

            var model =
                new AdminUserManagementViewModel
                {
                    TotalMembers =
                        members.Count,

                    ActiveMembers =
                        members.Count(x => !x.IsDisabled),

                    DisabledMembers =
                        members.Count(x => x.IsDisabled),

                    TotalMemberBalance =
                        members.Sum(x => x.CurrentBalance),

                    Members =
                        members
                };


            return View(model);
        }
        // -------------------------------------------------
        // Member Details
        // -------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> MemberDetails(string id)
        {
            // ---------------------------------------------
            // Validate User Id
            // ---------------------------------------------

            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Find User
            // ---------------------------------------------

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Make sure this user belongs to Member role
            // ---------------------------------------------

            var isMember =
                await _userManager.IsInRoleAsync(
                    user,
                    "Member");

            if (!isMember)
            {
                TempData["ErrorMessage"] =
                    "The selected user is not a member.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Get Member Transactions
            // ---------------------------------------------

            var transactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x =>
                        new AdminMemberTransactionViewModel
                        {
                            TransactionId =
                                x.TransactionId,

                            Amount =
                                x.Amount,

                            Type =
                                x.Type.ToString(),

                            Status =
                                x.Status.ToString(),

                            Category =
                                x.Category,

                            Purpose =
                                x.Purpose,

                            ReferenceNumber =
                                x.ReferenceNumber,

                            TransactionDate =
                                x.TransactionDate,

                            CreatedDate =
                                x.CreatedDate,

                            ApprovedDate =
                                x.ApprovedDate,

                            RejectionReason =
                                x.RejectionReason
                        })
                    .ToListAsync();


            // ---------------------------------------------
            // Calculate Approved Deposits
            // ---------------------------------------------

            var totalDeposited =
                transactions
                    .Where(x =>
                        x.Status ==
                            TransactionStatus.Approved.ToString() &&
                        x.Type ==
                            TransactionType.Deposit.ToString())
                    .Sum(x => x.Amount);


            // ---------------------------------------------
            // Calculate Approved Expenses
            // ---------------------------------------------

            var totalExpenses =
                transactions
                    .Where(x =>
                        x.Status ==
                            TransactionStatus.Approved.ToString() &&
                        x.Type ==
                            TransactionType.Withdrawal.ToString())
                    .Sum(x => x.Amount);


            // ---------------------------------------------
            // Pending Requests
            // ---------------------------------------------

            var pendingTransactions =
                transactions.Count(x =>
                    x.Status ==
                        TransactionStatus.Pending.ToString());


            // ---------------------------------------------
            // Identity Account Status
            // ---------------------------------------------

            var isDisabled =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value >
                    DateTimeOffset.UtcNow;


            // ---------------------------------------------
            // Build ViewModel
            // ---------------------------------------------

            var model =
                new AdminMemberDetailsViewModel
                {
                    UserId =
                        user.Id,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email ?? string.Empty,

                    PhoneNumber =
                        user.PhoneNumber,

                    CurrentBalance =
                        user.CurrentBalance,

                    IsDisabled =
                        isDisabled,

                    TotalDeposited =
                        totalDeposited,

                    TotalExpenses =
                        totalExpenses,

                    TotalTransactions =
                        transactions.Count,

                    PendingTransactions =
                        pendingTransactions,

                    Transactions =
                        transactions
                };


            return View(model);
        }
        // -------------------------------------------------
        // Disable Member Account
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableMember(string id)
        {
            // ---------------------------------------------
            // Validate User Id
            // ---------------------------------------------

            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Find User
            // ---------------------------------------------

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Protect Current Admin Account
            // ---------------------------------------------

            var currentAdminId =
                _userManager.GetUserId(User);

            if (user.Id == currentAdminId)
            {
                TempData["ErrorMessage"] =
                    "You cannot disable your own account.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Only Member accounts can be disabled here
            // ---------------------------------------------

            var isMember =
                await _userManager.IsInRoleAsync(
                    user,
                    "Member");

            if (!isMember)
            {
                TempData["ErrorMessage"] =
                    "Only member accounts can be disabled.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Already Disabled
            // ---------------------------------------------

            var alreadyDisabled =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value >
                    DateTimeOffset.UtcNow;

            if (alreadyDisabled)
            {
                TempData["ErrorMessage"] =
                    "This member account is already disabled.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Enable Identity Lockout
            // ---------------------------------------------

            var lockoutEnabledResult =
                await _userManager.SetLockoutEnabledAsync(
                    user,
                    true);

            if (!lockoutEnabledResult.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to enable account lockout.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Lock account indefinitely
            // ---------------------------------------------

            var lockoutResult =
                await _userManager.SetLockoutEndDateAsync(
                    user,
                    DateTimeOffset.MaxValue);

            if (!lockoutResult.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to disable the member account.";

                return RedirectToAction(nameof(Users));
            }


            TempData["SuccessMessage"] =
                $"{user.FullName}'s account has been disabled.";

            return RedirectToAction(nameof(Users));
        }


        // -------------------------------------------------
        // Enable Member Account
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableMember(string id)
        {
            // ---------------------------------------------
            // Validate User Id
            // ---------------------------------------------

            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Find User
            // ---------------------------------------------

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Only Member accounts can be enabled here
            // ---------------------------------------------

            var isMember =
                await _userManager.IsInRoleAsync(
                    user,
                    "Member");

            if (!isMember)
            {
                TempData["ErrorMessage"] =
                    "Only member accounts can be enabled.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Check Current Status
            // ---------------------------------------------

            var isDisabled =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value >
                    DateTimeOffset.UtcNow;

            if (!isDisabled)
            {
                TempData["ErrorMessage"] =
                    "This member account is already active.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Remove Lockout
            // ---------------------------------------------

            var result =
                await _userManager.SetLockoutEndDateAsync(
                    user,
                    null);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to enable the member account.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Reset Failed Login Count
            // ---------------------------------------------

            await _userManager.ResetAccessFailedCountAsync(
                user);


            TempData["SuccessMessage"] =
                $"{user.FullName}'s account has been enabled.";

            return RedirectToAction(nameof(Users));
        }
        // -------------------------------------------------
        // Delete Member Account
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(string id)
        {
            // ---------------------------------------------
            // Validate User Id
            // ---------------------------------------------

            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Find User
            // ---------------------------------------------

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Member could not be found.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Protect Current Admin Account
            // ---------------------------------------------

            var currentAdminId =
                _userManager.GetUserId(User);

            if (user.Id == currentAdminId)
            {
                TempData["ErrorMessage"] =
                    "You cannot delete your own account.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Only Member accounts can be deleted here
            // ---------------------------------------------

            var isMember =
                await _userManager.IsInRoleAsync(
                    user,
                    "Member");

            if (!isMember)
            {
                TempData["ErrorMessage"] =
                    "Only member accounts can be deleted.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Protect Wallet History
            // ---------------------------------------------

            var hasTransactions =
                await _context.WalletTransactions
                    .AsNoTracking()
                    .AnyAsync(x => x.UserId == user.Id);

            if (hasTransactions)
            {
                TempData["ErrorMessage"] =
                    "This member cannot be deleted because transaction history exists. Disable the account instead.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Member balance must be zero
            // ---------------------------------------------

            if (user.CurrentBalance != 0)
            {
                TempData["ErrorMessage"] =
                    "This member cannot be deleted because the wallet balance is not zero.";

                return RedirectToAction(nameof(Users));
            }


            // ---------------------------------------------
            // Delete Identity User
            // ---------------------------------------------

            var result =
                await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "Unable to delete the member account.";

                return RedirectToAction(nameof(Users));
            }


            TempData["SuccessMessage"] =
                $"{user.FullName}'s account has been deleted permanently.";

            return RedirectToAction(nameof(Users));
        }
        // Permanently clear wallet activity while preserving all accounts.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FreshStart(string confirmationText)
        {
            if (confirmationText != "DELETE WALLET DATA")
            {
                TempData["ErrorMessage"] =
                    "Fresh start cancelled. Please type DELETE WALLET DATA exactly.";
                return RedirectToAction(nameof(Dashboard));
            }

            try
            {
                await using var dbTransaction =
                    await _context.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable);

                if (!await _context.GroupWallets.AnyAsync())
                {
                    TempData["ErrorMessage"] = "Group wallet could not be found.";
                    return RedirectToAction(nameof(Dashboard));
                }

                var resetAt = DateTime.UtcNow;

                // Includes approved, pending and rejected requests.
                await _context.WalletTransactions.ExecuteDeleteAsync();

                // Only balances change; accounts, passwords and roles are preserved.
                await _context.Users.ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.CurrentBalance, 0m));

                await _context.GroupWallets.ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.TotalBalance, 0m)
                    .SetProperty(x => x.UpdatedAt, resetAt)
                    .SetProperty(x => x.LastResetAt, (DateTime?)resetAt));

                await dbTransaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Fresh start completed. All wallet transactions were deleted and all balances are now ₹0.00. Member accounts were preserved.";
            }
            catch (Exception ex)
            {
                // Disposing the uncommitted transaction rolls back all its changes.
                _context.ChangeTracker.Clear();
                HttpContext.RequestServices
                    .GetRequiredService<ILogger<AdminController>>()
                    .LogError(ex, "Wallet fresh start failed.");

                TempData["ErrorMessage"] =
                    "Fresh start failed. Please reload the dashboard to check the current wallet state before trying again.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // -------------------------------------------------
        // Reset Group Wallet
        // -------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetWallet(string confirmationText)
        {
            // ---------------------------------------------
            // Strong confirmation
            // ---------------------------------------------

            if (confirmationText != "RESET WALLET")
            {
                TempData["ErrorMessage"] =
                    "Wallet reset cancelled. Please type RESET WALLET exactly.";

                return RedirectToAction(nameof(Dashboard));
            }


            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -----------------------------------------
                // Do not reset while requests are pending
                // -----------------------------------------

                var hasPendingTransactions =
                    await _context.WalletTransactions
                        .AnyAsync(x =>
                            x.Status == TransactionStatus.Pending);

                if (hasPendingTransactions)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "The wallet cannot be reset while pending requests exist. Approve or reject them first.";

                    return RedirectToAction(nameof(Dashboard));
                }


                // -----------------------------------------
                // Find Group Wallet
                // -----------------------------------------

                var wallet =
                    await _context.GroupWallets
                        .FirstOrDefaultAsync();

                if (wallet == null)
                {
                    await dbTransaction.RollbackAsync();

                    TempData["ErrorMessage"] =
                        "Group wallet could not be found.";

                    return RedirectToAction(nameof(Dashboard));
                }


                // -----------------------------------------
                // Get Member Accounts
                // -----------------------------------------

                var memberUsers =
                    await _userManager
                        .GetUsersInRoleAsync("Member");


                // -----------------------------------------
                // Reset Group Balance
                // -----------------------------------------

                var resetAt = DateTime.UtcNow;

                wallet.TotalBalance = 0;
                wallet.UpdatedAt = resetAt;
                wallet.LastResetAt = resetAt;



                // -----------------------------------------
                // Reset Every Member Balance
                // -----------------------------------------

                foreach (var member in memberUsers)
                {
                    member.CurrentBalance = 0;
                }

                _context.Users.UpdateRange(memberUsers);


                // -----------------------------------------
                // Save Everything Together
                // -----------------------------------------

                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();


                TempData["SuccessMessage"] =
                    "Group wallet has been reset successfully. All member balances are now ₹0.00. Transaction history was preserved.";
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                _context.ChangeTracker.Clear();

                TempData["ErrorMessage"] =
                    "Something went wrong while resetting the wallet. No balance changes were applied.";
            }


            return RedirectToAction(nameof(Dashboard));
        }
    }
}