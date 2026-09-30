using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Models;

namespace ValavBoysWallet.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public DbSet<GroupWallet> GroupWallets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --------------------------------
            // Decimal precision
            // --------------------------------

            builder.Entity<ApplicationUser>()
                .Property(x => x.CurrentBalance)
                .HasPrecision(18, 2);

            builder.Entity<WalletTransaction>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Entity<GroupWallet>()
                .Property(x => x.TotalBalance)
                .HasPrecision(18, 2);

            // --------------------------------
            // User -> WalletTransactions
            // --------------------------------

            builder.Entity<WalletTransaction>()
                .HasOne(x => x.User)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------------------
            // ApprovedBy relationship
            // --------------------------------

            builder.Entity<WalletTransaction>()
                .HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------------------
            // Helpful indexes
            // --------------------------------

            builder.Entity<WalletTransaction>()
                .HasIndex(x => x.Status);

            builder.Entity<WalletTransaction>()
                .HasIndex(x => x.CreatedDate);
        }
    }
}