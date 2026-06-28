using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class NobetController : Controller
    {
        private readonly AppDbContext _db;

        public NobetController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis)
        {
            var query = _db.Nobetler
                .Include(n => n.Asistan)
                .Include(n => n.Bolum)
                .AsQueryable();

            if (baslangic.HasValue)
                query = query.Where(n => n.Tarih >= baslangic.Value);
            if (bitis.HasValue)
                query = query.Where(n => n.Tarih <= bitis.Value);

            var nobetler = await query
                .OrderByDescending(n => n.Tarih)
                .ThenBy(n => n.BaslangicSaati)
                .ToListAsync();

            ViewBag.Baslangic = baslangic?.ToString("yyyy-MM-dd");
            ViewBag.Bitis = bitis?.ToString("yyyy-MM-dd");
            return View(nobetler);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var nobet = await _db.Nobetler
                .Include(n => n.Asistan)
                .Include(n => n.Bolum)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nobet == null) return NotFound();
            return View(nobet);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new Nobet
            {
                Tarih = DateTime.Today,
                BaslangicSaati = new TimeSpan(8, 0, 0),
                BitisSaati = new TimeSpan(20, 0, 0)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nobet model)
        {
            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            // Cakisma kontrolu: Ayni asistan ayni gun ayni saatte
            var cakismaAsistan = await _db.Nobetler.AnyAsync(n =>
                n.AsistanId == model.AsistanId &&
                n.Tarih == model.Tarih &&
                n.BaslangicSaati == model.BaslangicSaati);
            if (cakismaAsistan)
                ModelState.AddModelError("", "Bu asistan ayni gun ayni saatte zaten nobet tutuyor.");

            // Cakisma kontrolu: Ayni bolum ayni gun ayni saatte iki asistan
            var cakismaBolum = await _db.Nobetler.AnyAsync(n =>
                n.BolumId == model.BolumId &&
                n.Tarih == model.Tarih &&
                n.BaslangicSaati == model.BaslangicSaati);
            if (cakismaBolum)
                ModelState.AddModelError("", "Bu bolumde ayni gun ayni saatte zaten nobetci var.");

            if (ModelState.IsValid)
            {
                _db.Nobetler.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Nobet eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(model.AsistanId, model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var nobet = await _db.Nobetler.FindAsync(id);
            if (nobet == null) return NotFound();
            await PopulateDropdowns(nobet.AsistanId, nobet.BolumId);
            return View(nobet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Nobet model)
        {
            if (id != model.Id) return NotFound();

            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Nobet guncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Nobetler.Any(n => n.Id == id)) return NotFound();
                    throw;
                }
            }
            await PopulateDropdowns(model.AsistanId, model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var nobet = await _db.Nobetler
                .Include(n => n.Asistan)
                .Include(n => n.Bolum)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nobet == null) return NotFound();
            return View(nobet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nobet = await _db.Nobetler.FindAsync(id);
            if (nobet != null)
            {
                _db.Nobetler.Remove(nobet);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Nobet silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? selectedAsistan = null, int? selectedBolum = null)
        {
            var asistanlar = await _db.Asistanlar
                .OrderBy(a => a.Ad)
                .Select(a => new { a.Id, AdSoyad = a.Ad + " " + a.Soyad })
                .ToListAsync();
            ViewBag.Asistanlar = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(asistanlar, "Id", "AdSoyad", selectedAsistan);

            var bolumler = await _db.Bolumler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.Bolumler = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(bolumler, "Id", "Ad", selectedBolum);
        }
    }
}
