using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public class Asistan
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        public string Telefon { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Adres { get; set; }

        [Display(Name = "Doğum Tarihi"), DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Asistanlık Yılı"), Range(1, 5)]
        public int AsistanlikYili { get; set; }

        [StringLength(500)]
        public string? FotoUrl { get; set; }

        [StringLength(1000)]
        public string? Biyografi { get; set; }

        public int? BolumId { get; set; }
        public Bolum? Bolum { get; set; }

        public ICollection<Nobet> Nobetler { get; set; } = new List<Nobet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}