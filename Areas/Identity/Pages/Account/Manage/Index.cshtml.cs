using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ValavBoysWallet.Models;
using ValavBoysWallet.Services;

namespace ValavBoysWallet.Areas.Identity.Pages.Account.Manage;

[Authorize]
[RequestSizeLimit(600_000)]
public class IndexModel(UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? Photo { get; set; }
    [TempData] public string? StatusMessage { get; set; }
    public string Email { get; set; } = "";
    public bool EmailConfirmed { get; set; }
    public bool HasPhoto { get; set; }
    public string Initial { get; set; } = "M";

    public class InputModel
    {
        [Required, StringLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = "";
        [Phone, StringLength(30), Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }

    private void Load(ApplicationUser user)
    {
        Email = user.Email ?? "";
        EmailConfirmed = user.EmailConfirmed;
        HasPhoto = user.ProfilePhoto is { Length: > 0 };
        Initial = string.IsNullOrWhiteSpace(user.FullName) ? "M" : user.FullName[..1].ToUpperInvariant();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        Load(user);
        Input = new() { FullName = user.FullName, PhoneNumber = user.PhoneNumber };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        Load(user);
        byte[]? photoBytes = null;
        string? contentType = null;
        if (Photo != null)
        {
            if (Photo.Length == 0 || Photo.Length > ProfilePhotoValidator.MaxBytes)
                ModelState.AddModelError("Photo", "Choose a JPG or PNG photo no larger than 512 KB.");
            else
            {
                using var stream = new MemoryStream();
                await Photo.CopyToAsync(stream);
                photoBytes = stream.ToArray();
                contentType = ProfilePhotoValidator.GetContentType(photoBytes);
                if (contentType == null)
                    ModelState.AddModelError("Photo", "Choose a valid JPG or PNG photo.");
            }
        }
        if (!ModelState.IsValid) return Page();
        user.FullName = Input.FullName.Trim();
        var phone = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim();
        if (phone != user.PhoneNumber)
        {
            user.PhoneNumber = phone;
            user.PhoneNumberConfirmed = false;
        }
        if (photoBytes != null)
        {
            user.ProfilePhoto = photoBytes;
            user.ProfilePhotoContentType = contentType;
        }
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        await signIn.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemovePhotoAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        user.ProfilePhoto = null;
        user.ProfilePhotoContentType = null;
        var result = await users.UpdateAsync(user);
        StatusMessage = result.Succeeded ? "Profile photo removed." : "Unable to remove the photo. Please try again.";
        return RedirectToPage();
    }
}
