using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public class Bolum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bölüm adı zorunludur")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [Display(Name = "Mevcut Hasta Sayısı")]
        public int HastaSayisi { get; set; }

        [Display(Name = "Boş Yatak Sayısı")]
        public int BosYatakSayisi { get; set; }

        [Display(Name = "Toplam Yatak Kapasitesi")]
        public int ToplamYatakSayisi { get; set; }

        [StringLength(500)]
        public string? GorselUrl { get; set; }

        public int? SorumluOgretimUyesiId { get; set; }
        public OgretimUyesi? SorumluOgretimUyesi { get; set; }

        public ICollection<Asistan> Asistanlar { get; set; } = new List<Asistan>();
        public ICollection<Nobet> Nobetler { get; set; } = new List<Nobet>();
    }
}