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
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;


        public TransactionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }


        // ---------------------------------------------
        // Deposit GET
        // ---------------------------------------------

        [HttpGet]
        public IActionResult Deposit()
        {
            var model = new DepositRequestViewModel
            {
                TransactionDate = DateTime.Today
            };

            return View(model);
        }


        // ---------------------------------------------
        // Deposit POST
        // ---------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(
            DepositRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // -----------------------------------------
            // Clean reference number
            // -----------------------------------------

            var referenceNumber =
                model.ReferenceNumber.Trim();


            // -----------------------------------------
            // Check duplicate reference
            // -----------------------------------------

            var duplicateReference =
                await _context.WalletTransactions
                    .AnyAsync(x =>
                        x.Type == TransactionType.Deposit &&
                        x.ReferenceNumber == referenceNumber);


            if (duplicateReference)
            {
                ModelState.AddModelError(
                    nameof(model.ReferenceNumber),
                    "This reference number has already been submitted.");

                return View(model);
            }


            // -----------------------------------------
            // Validate screenshot
            // -----------------------------------------

            if (model.Screenshot == null ||
                model.Screenshot.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Screenshot),
                    "Please upload the payment screenshot.");

                return View(model);
            }


            const long maxFileSize =
                5 * 1024 * 1024;


            if (model.Screenshot.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(model.Screenshot),
                    "Screenshot must be 5 MB or smaller.");

                return View(model);
            }


            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };


            var extension =
                Path.GetExtension(
                    model.Screenshot.FileName)
                    .ToLowerInvariant();


            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(model.Screenshot),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return View(model);
            }


            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };


            if (!allowedContentTypes.Contains(
                    model.Screenshot.ContentType
                        .ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    nameof(model.Screenshot),
                    "The uploaded file must be a valid image.");

                return View(model);
            }


            // -----------------------------------------
            // Create uploads folder
            // -----------------------------------------

            var uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "deposits");


            Directory.CreateDirectory(
                uploadsFolder);


            // -----------------------------------------
            // Generate safe random filename
            // -----------------------------------------

            var fileName =
                $"{Guid.NewGuid():N}{extension}";


            var physicalFilePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);


            // -----------------------------------------
            // Save screenshot
            // -----------------------------------------

            await using (
                var fileStream =
                    new FileStream(
                        physicalFilePath,
                        FileMode.Create))
            {
                await model.Screenshot
                    .CopyToAsync(fileStream);
            }


            // -----------------------------------------
            // Store only relative URL in database
            // -----------------------------------------

            var screenshotPath =
                $"/uploads/deposits/{fileName}";


            try
            {
                // -------------------------------------
                // Create Pending Deposit
                // -------------------------------------

                var transaction =
                 new WalletTransaction
                 {
                     UserId = userId,

                        Amount = model.Amount,

                        Type =
                        TransactionType.Deposit,

                        ReferenceNumber =
                        referenceNumber,

                        ScreenshotPath =
                        screenshotPath,

                        TransactionDate =
                        model.TransactionDate.Date,

                        Status =
                        TransactionStatus.Pending,
    
                        CreatedDate =
                        DateTime.UtcNow
                  };


                _context.WalletTransactions
                    .Add(transaction);


                await _context.SaveChangesAsync();
            }
            catch
            {
                // If database save fails,
                // remove the uploaded file.

                if (System.IO.File.Exists(
                        physicalFilePath))
                {
                    System.IO.File.Delete(
                        physicalFilePath);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to submit the deposit request. Please try again.");

                return View(model);
            }


            TempData["SuccessMessage"] =
                "Deposit request submitted successfully. " +
                "Waiting for admin approval.";


            return RedirectToAction(
                nameof(MyTransactions));
        }


        // ---------------------------------------------
        // My Transactions
        // ---------------------------------------------

        [HttpGet]
        public async Task<IActionResult> MyTransactions()
        {
            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            var transactions =
                await _context.WalletTransactions
                    .Where(x =>
                        x.UserId == userId)
                    .OrderByDescending(x =>
                        x.CreatedDate)
                    .ToListAsync();


            return View(transactions);
        }


        // ---------------------------------------------
        // Withdrawal GET
        // ---------------------------------------------

        [HttpGet]
        public IActionResult Withdrawal()
        {
            return View(
                new WithdrawalRequestViewModel());
        }


        // ---------------------------------------------
        // Withdrawal POST
        // ---------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(
            WithdrawalRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var userId =
                _userManager.GetUserId(User);


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.Id == userId);


            if (user == null)
            {
                return NotFound();
            }


            if (model.Amount >
                user.CurrentBalance)
            {
                ModelState.AddModelError(
                    nameof(model.Amount),
                    "You cannot request more than " +
                    "your current available balance.");

                return View(model);
            }


            var transaction =
                new WalletTransaction
                {
                    UserId = userId,

                    Amount = model.Amount,

                    Type =
                        TransactionType.Withdrawal,

                    Category =
                        model.Category.Trim(),

                    Purpose =
                        model.Purpose.Trim(),

                    Status =
                        TransactionStatus.Pending,

                    CreatedDate =
                        DateTime.UtcNow
                };


            _context.WalletTransactions
                .Add(transaction);


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Expense request submitted successfully. " +
                "Waiting for admin approval.";


            return RedirectToAction(
                nameof(MyTransactions));
        }
    }
}