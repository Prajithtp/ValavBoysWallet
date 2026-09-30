using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using ValavBoysWallet.Models;

namespace ValavBoysWallet.Areas.Identity.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting("account-recovery")]
public class ForgotPasswordModel(UserManager<ApplicationUser> users,
    IEmailSender sender, IConfiguration configuration, ILogger<ForgotPasswordModel> logger) : PageModel
{
    [BindProperty, Required, EmailAddress] public string Email { get; set; } = "";
    public bool Submitted { get; set; }
    public bool EmailEnabled => configuration.GetValue<bool>("Email:Enabled");
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!EmailEnabled) return Page();
        if (!ModelState.IsValid) return Page();
        var baseUrl = configuration["Email:PublicBaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var origin) ||
            origin.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(origin.UserInfo) ||
            !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment) ||
            new[] { "SmtpHost", "SmtpUsername", "SmtpPassword", "FromAddress" }
                .Any(key => string.IsNullOrWhiteSpace(configuration[$"Email:{key}"])))
        {
            ModelState.AddModelError("", "Password recovery is not available yet. Please contact the administrator.");
            return Page();
        }

        var user = await users.FindByEmailAsync(Email.Trim());
        if (user != null && await users.IsEmailConfirmedAsync(user))
        {
            try
            {
                var code = await users.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var path = Url.Page("/Account/ResetPassword", pageHandler: null,
                    values: new { area = "Identity", code });
                if (path == null) throw new InvalidOperationException("Reset page route missing.");
                var link = new Uri(origin, path).AbsoluteUri;
                await sender.SendEmailAsync(user.Email!, "Reset your wallet password",
                    "<p>A password reset was requested for your Valav Boys Wallet account.</p>" +
                    $"<p><a href=\"{HtmlEncoder.Default.Encode(link)}\">Reset your password</a></p>" +
                    "<p>This link expires in one hour. If you did not request this, ignore this email.</p>");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Password recovery email could not be sent.");
            }
        }
        // Same response for unknown, unverified and eligible addresses.
        Submitted = true;
        return Page();
    }
}
