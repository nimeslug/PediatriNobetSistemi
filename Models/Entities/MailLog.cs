namespace PediatriNobetSistemi.Models.Entities
{
    public class MailLog
    {
        public int Id { get; set; }
        public string AliciEmail { get; set; } = string.Empty;
        public string Konu { get; set; } = string.Empty;
        public DateTime GonderimTarihi { get; set; } = DateTime.Now;
        public bool Basarili { get; set; }
        public string? HataMesaji { get; set; }
        public int? AcilDurumHaberiId { get; set; }
    }
}