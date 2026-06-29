using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;

namespace PediatriNobetSistemi.Controllers
{
    public class AcilDurumController : Controller
    {
        private readonly AppDbContext _db;

        public AcilDurumController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var haberler = await _db.AcilDurumHaberleri
                .Include(a => a.Bolum)
                .OrderByDescending(a => a.YayinTarihi)
                .Take(50)
                .ToListAsync();
            return View(haberler);
        }
    }
}
