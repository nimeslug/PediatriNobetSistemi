using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;
using PediatriNobetSistemi.Services;

namespace PediatriNobetSistemi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SeedData.AdminRole)]
    public class AcilDurumController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IMailService _mail;
        private readonly ILogger<AcilDurumController> _logger;

        public AcilDurumController(AppDbContext db, IMailService mail, ILogger<AcilDurumController> logger)
        {
            _db = db;
            _mail = mail;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var haberler = await _db.AcilDurumHaberleri
                .Include(a => a.Bolum)
                .OrderByDescending(a => a.YayinTarihi)
                .ToListAsync();
            return View(haberler);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateBolumDropdown();
            return View(new AcilDurumHaberi { YayinTarihi = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcilDurumHaberi model, bool gonderMail = true)
        {
            if (ModelState.IsValid)
            {
                _db.AcilDurumHaberleri.Add(model);
                await _db.SaveChangesAsync();

                if (gonderMail)
                {
                    await TumEkibeMailGonder(model);
                    TempData["Success"] = "Haber eklendi. Mail gonderimi denendi (loglar icin MailLoglari tablosuna bakin).";
                }
                else
                {
                    TempData["Success"] = "Haber eklendi (mail gonderilmedi).";
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var haber = await _db.AcilDurumHaberleri.FindAsync(id);
            if (haber == null) return NotFound();
            await PopulateBolumDropdown(haber.BolumId);
            return View(haber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcilDurumHaberi model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Haber guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateBolumDropdown(model.BolumId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var haber = await _db.AcilDurumHaberleri
                .Include(a => a.Bolum)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (haber == null) return NotFound();
            return View(haber);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var haber = await _db.AcilDurumHaberleri.FindAsync(id);
            if (haber != null)
            {
                _db.AcilDurumHaberleri.Remove(haber);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Haber silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MailTekrarGonder(int id)
        {
            var haber = await _db.AcilDurumHaberleri.FindAsync(id);
            if (haber != null)
            {
                await TumEkibeMailGonder(haber);
                TempData["Success"] = "Mail gonderimi tekrar denendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MailLoglari(int id)
        {
            var loglar = await _db.MailLoglari
                .Where(l => l.AcilDurumHaberiId == id)
                .OrderByDescending(l => l.GonderimTarihi)
                .ToListAsync();
            ViewBag.HaberId = id;
            return View(loglar);
        }

        private async Task TumEkibeMailGonder(AcilDurumHaberi haber)
        {
            // Asistan ve ogretim uyelerinin mail adreslerini topla
            var asistanMailler = await _db.Asistanlar
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email).ToListAsync();

            var hocaMailler = await _db.OgretimUyeleri
                .Where(o => !string.IsNullOrEmpty(o.Email))
                .Select(o => o.Email).ToListAsync();

            var tumMailler = asistanMailler.Concat(hocaMailler).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (!tumMailler.Any())
            {
                _logger.LogWarning("Gonderilecek mail adresi bulunamadi.");
                return;
            }

            var konu = $"[{haber.Oncelik}] {haber.Baslik}";
            var govde = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <div style='background-color: #e74c3c; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                        <h2 style='margin: 0;'>🚨 Acil Durum Bildirimi</h2>
                        <p style='margin: 5px 0 0 0; font-size: 14px;'>Oncelik: <strong>{haber.Oncelik}</strong></p>
                    </div>
                    <div style='border: 1px solid #ddd; border-top: none; padding: 20px; border-radius: 0 0 5px 5px;'>
                        <h3>{haber.Baslik}</h3>
                        <p style='white-space: pre-wrap;'>{haber.Icerik}</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                        <p style='color: #888; font-size: 12px;'>
                            Yayin tarihi: {haber.YayinTarihi:dd.MM.yyyy HH:mm}<br />
                            Bu mail Pediatri Nobet Sistemi tarafindan otomatik olarak gonderilmistir.
                        </p>
                    </div>
                </div>";

            bool tumuBasarili = true;
            foreach (var mail in tumMailler)
            {
                bool basarili = await _mail.SendAsync(mail, konu, govde);
                _db.MailLoglari.Add(new MailLog
                {
                    AliciEmail = mail,
                    Konu = konu,
                    Basarili = basarili,
                    HataMesaji = basarili ? null : "Gonderim basarisiz (SMTP ayarlarini kontrol edin)",
                    AcilDurumHaberiId = haber.Id
                });
                if (!basarili) tumuBasarili = false;
            }

            haber.MailGonderildi = tumuBasarili && tumMailler.Any();
            await _db.SaveChangesAsync();
        }

        private async Task PopulateBolumDropdown(int? selected = null)
        {
            var liste = await _db.Bolumler.OrderBy(b => b.Ad).ToListAsync();
            ViewBag.Bolumler = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(liste, "Id", "Ad", selected);
        }
    }
}
