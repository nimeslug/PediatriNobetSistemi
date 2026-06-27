using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class BolumController : Controller
    {
        private readonly AppDbContext _db;

        public BolumController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Bolum
        public async Task<IActionResult> Index()
        {
            var bolumler = await _db.Bolumler
                .Include(b => b.SorumluOgretimUyesi)
                .OrderBy(b => b.Ad)
                .ToListAsync();
            return View(bolumler);
        }

        // GET: /Admin/Bolum/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bolum = await _db.Bolumler
                .Include(b => b.SorumluOgretimUyesi)
                .Include(b => b.Asistanlar)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bolum == null) return NotFound();
            return View(bolum);
        }

        // GET: /Admin/Bolum/Create
        public async Task<IActionResult> Create()
        {
            await PopulateOgretimUyesiDropdown();
            return View();
        }

        // POST: /Admin/Bolum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bolum model)
        {
            if (ModelState.IsValid)
            {
                _db.Bolumler.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Bolum basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateOgretimUyesiDropdown(model.SorumluOgretimUyesiId);
            return View(model);
        }

        // GET: /Admin/Bolum/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var bolum = await _db.Bolumler.FindAsync(id);
            if (bolum == null) return NotFound();
            await PopulateOgretimUyesiDropdown(bolum.SorumluOgretimUyesiId);
            return View(bolum);
        }

        // POST: /Admin/Bolum/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bolum model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Bolum guncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Bolumler.Any(b => b.Id == id)) return NotFound();
                    throw;
                }
            }
            await PopulateOgretimUyesiDropdown(model.SorumluOgretimUyesiId);
            return View(model);
        }

        // GET: /Admin/Bolum/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var bolum = await _db.Bolumler
                .Include(b => b.SorumluOgretimUyesi)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bolum == null) return NotFound();
            return View(bolum);
        }

        // POST: /Admin/Bolum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bolum = await _db.Bolumler.FindAsync(id);
            if (bolum != null)
            {
                _db.Bolumler.Remove(bolum);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Bolum silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateOgretimUyesiDropdown(int? selected = null)
        {
            var liste = await _db.OgretimUyeleri
                .OrderBy(o => o.Ad)
                .Select(o => new { o.Id, AdSoyad = o.Unvan + " " + o.Ad + " " + o.Soyad })
                .ToListAsync();
            ViewBag.OgretimUyeleri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(liste, "Id", "AdSoyad", selected);
        }
    }
}
