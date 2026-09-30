using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Models;

namespace ValavBoysWallet.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply any pending migrations
            await context.Database.MigrateAsync();

            // --------------------------------
            // Roles
            // --------------------------------

            const string adminRole = "Admin";
            const string memberRole = "Member";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(memberRole))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(memberRole));
            }

            // --------------------------------
            // Default Admin
            // --------------------------------

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var existingAdmins = await userManager.GetUsersInRoleAsync(adminRole);
            if (existingAdmins.Count == 0)
            {
                var adminEmail = configuration["BootstrapAdmin:Email"]?.Trim();
                var adminPassword = configuration["BootstrapAdmin:Password"];
                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(adminEmail) ||
                    string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Length < 12 ||
                    adminPassword.StartsWith("REPLACE_", StringComparison.Ordinal))
                    throw new InvalidOperationException("Set BootstrapAdmin:Email and a private password of at least 12 characters before first startup.");

                // Never promote an existing self-registered account by email alone.
                if (await userManager.FindByEmailAsync(adminEmail) != null)
                    throw new InvalidOperationException("The bootstrap email already belongs to an account. Choose a new administrator email.");

                await using var transaction = await context.Database.BeginTransactionAsync();
                var admin = new ApplicationUser
                {
                    FullName = "Valav Boys Wallet Admin", UserName = adminEmail,
                    Email = adminEmail, EmailConfirmed = false, CurrentBalance = 0
                };
                var created = await userManager.CreateAsync(admin, adminPassword);
                if (!created.Succeeded)
                    throw new InvalidOperationException("Admin creation failed: " + string.Join(", ", created.Errors.Select(e => e.Description)));
                var assigned = await userManager.AddToRoleAsync(admin, adminRole);
                if (!assigned.Succeeded)
                    throw new InvalidOperationException("Administrator role assignment failed.");
                await transaction.CommitAsync();
            }

            // --------------------------------
            // Initial Group Wallet
            // --------------------------------

            if (!await context.GroupWallets.AnyAsync())
            {
                var wallet = new GroupWallet
                {
                    WalletName = "Valav Boys Wallet",
                    TotalBalance = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                context.GroupWallets.Add(wallet);

                await context.SaveChangesAsync();
            }
        }
    }
}