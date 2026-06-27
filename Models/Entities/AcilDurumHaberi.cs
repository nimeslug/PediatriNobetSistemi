using System.ComponentModel.DataAnnotations;

namespace PediatriNobetSistemi.Models.Entities
{
    public enum OncelikDuzeyi
    {
        Dusuk = 0,
        Orta = 1,
        Yuksek = 2,
        Kritik = 3
    }

    public class AcilDurumHaberi
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Baslik { get; set; } = string.Empty;

        [Required, StringLength(4000)]
        public string Icerik { get; set; } = string.Empty;

        public OncelikDuzeyi Oncelik { get; set; } = OncelikDuzeyi.Orta;

        public DateTime YayinTarihi { get; set; } = DateTime.Now;

        public bool MailGonderildi { get; set; } = false;

        public int? BolumId { get; set; }
        public Bolum? Bolum { get; set; }
    }
}