using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class MusaitlikController : Controller
    {
        private readonly AppDbContext _db;

        public MusaitlikController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? ogretimUyesiId)
        {
            var query = _db.Musaitlikler.Include(m => m.OgretimUyesi).AsQueryable();
            if (ogretimUyesiId.HasValue)
                query = query.Where(m => m.OgretimUyesiId == ogretimUyesiId.Value);

            var liste = await query.OrderBy(m => m.OgretimUyesiId).ThenBy(m => m.Gun).ThenBy(m => m.BaslangicSaati).ToListAsync();

            ViewBag.OgretimUyeleri = await _db.OgretimUyeleri.OrderBy(o => o.Ad)
                .Select(o => new { o.Id, AdSoyad = o.Unvan + " " + o.Ad + " " + o.Soyad }).ToListAsync();
            ViewBag.SecilenOgretimUyesi = ogretimUyesiId;

            return View(liste);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdown();
            return View(new Musaitlik { BaslangicSaati = new TimeSpan(14,0,0), BitisSaati = new TimeSpan(14,30,0), Aktif = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Musaitlik model)
        {
            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (ModelState.IsValid)
            {
                _db.Musaitlikler.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Musaitlik eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdown(model.OgretimUyesiId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Musaitlikler.FindAsync(id);
            if (m == null) return NotFound();
            await PopulateDropdown(m.OgretimUyesiId);
            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Musaitlik model)
        {
            if (id != model.Id) return NotFound();

            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (ModelState.IsValid)
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Musaitlik guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdown(model.OgretimUyesiId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Musaitlikler.Include(x => x.OgretimUyesi).FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return NotFound();
            return View(m);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var m = await _db.Musaitlikler.FindAsync(id);
            if (m != null)
            {
                _db.Musaitlikler.Remove(m);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Musaitlik silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdown(int? selected = null)
        {
            var liste = await _db.OgretimUyeleri.OrderBy(o => o.Ad)
                .Select(o => new { o.Id, AdSoyad = o.Unvan + " " + o.Ad + " " + o.Soyad }).ToListAsync();
            ViewBag.OgretimUyeleri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(liste, "Id", "AdSoyad", selected);
        }
    }
}
