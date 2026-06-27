using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class OgretimUyesiController : Controller
    {
        private readonly AppDbContext _db;

        public OgretimUyesiController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var hocalar = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .OrderBy(o => o.Ad)
                .ToListAsync();
            return View(hocalar);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (hoca == null) return NotFound();
            return View(hoca);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateBolumDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OgretimUyesi model)
        {
            if (ModelState.IsValid)
            {
                _db.OgretimUyeleri.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Ogretim uyesi eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hoca = await _db.OgretimUyeleri.FindAsync(id);
            if (hoca == null) return NotFound();
            await PopulateBolumDropdown(hoca.BolumId);
            return View(hoca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OgretimUyesi model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Ogretim uyesi guncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.OgretimUyeleri.Any(o => o.Id == id)) return NotFound();
                    throw;
                }
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (hoca == null) return NotFound();
            return View(hoca);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoca = await _db.OgretimUyeleri.FindAsync(id);
            if (hoca != null)
            {
                _db.OgretimUyeleri.Remove(hoca);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Ogretim uyesi silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateBolumDropdown(int? selected = null)
        {
            var liste = await _db.Bolumler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.Bolumler = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(liste, "Id", "Ad", selected);
        }
    }
}
