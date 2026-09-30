using Microsoft.AspNetCore.Identity.UI.Services;
using ValavBoysWallet.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Data;
using ValavBoysWallet.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------
// Database
// --------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --------------------------------------------
// Identity
// --------------------------------------------

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --------------------------------------------
// MVC
// --------------------------------------------

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailSender, SesSmtpEmailSender>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(1));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("account-recovery", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0
            }));
});

// This hosting plan cannot deliver email. Block built-in email workflows too.
var emailEnabled = builder.Configuration.GetValue<bool>("Email:Enabled");
var app = builder.Build();
if (!emailEnabled)
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value?.TrimEnd('/');
        if (string.Equals(path, "/Identity/Account/Manage/Email", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, "/Identity/Account/ResendEmailConfirmation", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, "/Identity/Account/RegisterConfirmation", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect(context.Request.PathBase + "/Identity/Account/ForgotPassword");
            return;
        }
        await next();
    });
}

// --------------------------------------------
// HTTP Pipeline
// --------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve files from wwwroot.
// This also supports runtime-uploaded payment screenshots.
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

// Identity authentication must run before authorization.
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

await DbSeeder.SeedAsync(app.Services);

app.Run();