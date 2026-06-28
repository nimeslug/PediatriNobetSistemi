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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events(DateTime start, DateTime end)
        {
            // Sadece tarih kismini al, saat farklarini iptal et
            var baslangic = start.Date;
            var bitis = end.Date.AddDays(1);

            var nobetler = await _db.Nobetler
                .Include(n => n.Asistan)
                .Include(n => n.Bolum)
                .Where(n => n.Tarih >= baslangic && n.Tarih < bitis)
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

        private static string GetBolumColor(int bolumId)
        {
            var palet = new[]
            {
                "#e74c3c", "#3498db", "#2ecc71", "#f39c12",
                "#9b59b6", "#1abc9c", "#34495e", "#e67e22"
            };
            return palet[bolumId % palet.Length];
        }
    }
}
