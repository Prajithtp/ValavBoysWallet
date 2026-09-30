using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ValavBoysWallet.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

        public byte[]? ProfilePhoto { get; set; }

        [StringLength(32)]
        public string? ProfilePhotoContentType { get; set; }

        public ICollection<WalletTransaction> Transactions { get; set; }
            = new List<WalletTransaction>();
    }
}