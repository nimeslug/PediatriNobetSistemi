using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public class Nobet
    {
        public int Id { get; set; }

        [Required]
        public int AsistanId { get; set; }
        public Asistan? Asistan { get; set; }

        [Required]
        public int BolumId { get; set; }
        public Bolum? Bolum { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Nöbet Tarihi")]
        public DateTime Tarih { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan BitisSaati { get; set; }

        [StringLength(500)]
        public string? Notlar { get; set; }
    }
}