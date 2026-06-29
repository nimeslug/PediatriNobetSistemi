using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;

namespace PediatriNobetSistemi.Controllers
{
    public class OgretimUyeleriController : Controller
    {
        private readonly AppDbContext _db;
        public OgretimUyeleriController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var hocalar = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .OrderBy(o => o.Ad)
                .ToListAsync();
            return View(hocalar);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .Include(o => o.Musaitlikler.Where(m => m.Aktif))
                .FirstOrDefaultAsync(o => o.Id == id);
            if (hoca == null) return NotFound();
            return View(hoca);
        }
    }
}
