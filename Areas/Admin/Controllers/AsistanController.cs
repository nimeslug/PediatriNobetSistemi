using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class AsistanController : Controller
    {
        private readonly AppDbContext _db;

        public AsistanController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var asistanlar = await _db.Asistanlar
                .Include(a => a.Bolum)
                .OrderBy(a => a.Ad)
                .ToListAsync();
            return View(asistanlar);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var asistan = await _db.Asistanlar
                .Include(a => a.Bolum)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (asistan == null) return NotFound();
            return View(asistan);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateBolumDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asistan model)
        {
            if (ModelState.IsValid)
            {
                _db.Asistanlar.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Asistan basariyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var asistan = await _db.Asistanlar.FindAsync(id);
            if (asistan == null) return NotFound();
            await PopulateBolumDropdown(asistan.BolumId);
            return View(asistan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Asistan model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Asistan guncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Asistanlar.Any(a => a.Id == id)) return NotFound();
                    throw;
                }
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var asistan = await _db.Asistanlar
                .Include(a => a.Bolum)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (asistan == null) return NotFound();
            return View(asistan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asistan = await _db.Asistanlar.FindAsync(id);
            if (asistan != null)
            {
                _db.Asistanlar.Remove(asistan);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Asistan silindi.";
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
