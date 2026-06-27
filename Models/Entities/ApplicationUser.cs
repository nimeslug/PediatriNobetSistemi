using Microsoft.AspNetCore.Identity;

namespace PediatriNobetSistemi.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
    }
}