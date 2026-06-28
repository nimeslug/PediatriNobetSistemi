using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;

namespace PediatriNobetSistemi.Controllers
{
    public class TakvimController : Controller
    {
        private readonly AppDbContext _db;

        public TakvimController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Takvim
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Takvim/Events?start=...&end=...
        // FullCalendar bu endpoint'ten JSON event listesi ister
        [HttpGet]
        public async Task<IActionResult> Events(DateTime start, DateTime end)
        {
            var nobetler = await _db.Nobetler
                .Include(n => n.Asistan)
                .Include(n => n.Bolum)
                .Where(n => n.Tarih >= start && n.Tarih <= end)
                .ToListAsync();

            var events = nobetler.Select(n => new
            {
                id = n.Id,
                title = $"{n.Asistan?.Ad} {n.Asistan?.Soyad} — {n.Bolum?.Ad}",
                start = n.Tarih.Add(n.BaslangicSaati).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = n.Tarih.Add(n.BitisSaati).ToString("yyyy-MM-ddTHH:mm:ss"),
                color = GetBolumColor(n.BolumId),
                extendedProps = new
                {
                    asistan = $"{n.Asistan?.Ad} {n.Asistan?.Soyad}",
                    bolum = n.Bolum?.Ad,
                    notlar = n.Notlar
                }
            });

            return Json(events);
        }

        // Bolum ID'sine gore renk uretir (tutarli ve farkli renkler)
        private static string GetBolumColor(int bolumId)
        {
            var palet = new[]
            {
                "#e74c3c", // kirmizi
                "#3498db", // mavi
                "#2ecc71", // yesil
                "#f39c12", // turuncu
                "#9b59b6", // mor
                "#1abc9c", // turkuaz
                "#34495e", // koyu lacivert
                "#e67e22"  // koyu turuncu
            };
            return palet[bolumId % palet.Length];
        }
    }
}
