using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class RandevuController : Controller
    {
        private readonly AppDbContext _db;

        public RandevuController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(RandevuDurumu? durum)
        {
            var query = _db.Randevular
                .Include(r => r.Asistan)
                .Include(r => r.OgretimUyesi)
                .AsQueryable();

            if (durum.HasValue)
                query = query.Where(r => r.Durum == durum.Value);

            var liste = await query
                .OrderByDescending(r => r.Tarih)
                .ThenBy(r => r.BaslangicSaati)
                .ToListAsync();

            ViewBag.SecilenDurum = durum;
            return View(liste);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var r = await _db.Randevular
                .Include(x => x.Asistan)
                .Include(x => x.OgretimUyesi)
                .Include(x => x.Musaitlik)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return View(r);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new Randevu { Tarih = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Randevu model)
        {
            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (ModelState.IsValid)
            {
                _db.Randevular.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Randevu eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(model.AsistanId, model.OgretimUyesiId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var r = await _db.Randevular.FindAsync(id);
            if (r == null) return NotFound();
            await PopulateDropdowns(r.AsistanId, r.OgretimUyesiId);
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Randevu model)
        {
            if (id != model.Id) return NotFound();
            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (ModelState.IsValid)
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Randevu guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(model.AsistanId, model.OgretimUyesiId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var r = await _db.Randevular
                .Include(x => x.Asistan)
                .Include(x => x.OgretimUyesi)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return View(r);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var r = await _db.Randevular.FindAsync(id);
            if (r != null)
            {
                _db.Randevular.Remove(r);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Randevu silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DurumDegistir(int id, RandevuDurumu durum)
        {
            var r = await _db.Randevular.FindAsync(id);
            if (r != null)
            {
                r.Durum = durum;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Durum guncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? selectedAsistan = null, int? selectedHoca = null)
        {
            var asistanlar = await _db.Asistanlar.OrderBy(a => a.Ad)
                .Select(a => new { a.Id, AdSoyad = a.Ad + " " + a.Soyad }).ToListAsync();
            ViewBag.Asistanlar = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(asistanlar, "Id", "AdSoyad", selectedAsistan);

            var hocalar = await _db.OgretimUyeleri.OrderBy(o => o.Ad)
                .Select(o => new { o.Id, AdSoyad = o.Unvan + " " + o.Ad + " " + o.Soyad }).ToListAsync();
            ViewBag.OgretimUyeleri = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hocalar, "Id", "AdSoyad", selectedHoca);
        }
    }
}
