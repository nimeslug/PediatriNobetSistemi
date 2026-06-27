using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public class OgretimUyesi
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Unvan { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        public string Telefon { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Adres { get; set; }

        [StringLength(2000)]
        public string? Ozgecmis { get; set; }

        [StringLength(500)]
        public string? UzmanlikAlani { get; set; }

        [StringLength(500)]
        public string? FotoUrl { get; set; }

        public int? BolumId { get; set; }
        public Bolum? Bolum { get; set; }

        public ICollection<Musaitlik> Musaitlikler { get; set; } = new List<Musaitlik>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}