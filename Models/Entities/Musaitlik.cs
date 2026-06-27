using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public class Musaitlik
    {
        public int Id { get; set; }

        [Required]
        public int OgretimUyesiId { get; set; }
        public OgretimUyesi? OgretimUyesi { get; set; }

        [Required, Display(Name = "Gün")]
        public DayOfWeek Gun { get; set; }

        [Required, Display(Name = "Başlangıç")]
        public TimeSpan BaslangicSaati { get; set; }

        [Required, Display(Name = "Bitiş")]
        public TimeSpan BitisSaati { get; set; }

        public bool Aktif { get; set; } = true;
    }
}