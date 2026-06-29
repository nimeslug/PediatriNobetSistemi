using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;

namespace PediatriNobetSistemi.Controllers
{
    public class BolumlerController : Controller
    {
        private readonly AppDbContext _db;
        public BolumlerController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var bolumler = await _db.Bolumler
                .Include(b => b.SorumluOgretimUyesi)
                .Include(b => b.Asistanlar)
                .OrderBy(b => b.Ad)
                .ToListAsync();
            return View(bolumler);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var bolum = await _db.Bolumler
                .Include(b => b.SorumluOgretimUyesi)
                .Include(b => b.Asistanlar)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bolum == null) return NotFound();
            return View(bolum);
        }
    }
}
