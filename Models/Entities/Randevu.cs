using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public enum RandevuDurumu
    {
        Beklemede = 0,
        Onaylandi = 1,
        Iptal = 2,
        Tamamlandi = 3
    }

    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public int AsistanId { get; set; }
        public Asistan? Asistan { get; set; }

        [Required]
        public int OgretimUyesiId { get; set; }
        public OgretimUyesi? OgretimUyesi { get; set; }

        public int? MusaitlikId { get; set; }
        public Musaitlik? Musaitlik { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        public TimeSpan BitisSaati { get; set; }

        [StringLength(500)]
        [Display(Name = "Görüşme Konusu")]
        public string? Konu { get; set; }

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}