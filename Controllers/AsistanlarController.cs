using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;

namespace PediatriNobetSistemi.Controllers
{
    public class AsistanlarController : Controller
    {
        private readonly AppDbContext _db;
        public AsistanlarController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var asistanlar = await _db.Asistanlar
                .Include(a => a.Bolum)
                .OrderBy(a => a.AsistanlikYili)
                .ThenBy(a => a.Ad)
                .ToListAsync();
            return View(asistanlar);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var asistan = await _db.Asistanlar
                .Include(a => a.Bolum)
                .Include(a => a.Nobetler.OrderByDescending(n => n.Tarih).Take(10))
                    .ThenInclude(n => n.Bolum)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (asistan == null) return NotFound();
            return View(asistan);
        }
    }
}
