using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Data;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RandevuController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null)
            {
                TempData["Warning"] = "Hesabiniz bir asistan ile eslesmemis. Randevu olusturabilmek icin admin ile gorusun.";
                return View(new List<Randevu>());
            }

            var randevular = await _db.Randevular
                .Include(r => r.OgretimUyesi)
                .Where(r => r.AsistanId == asistan.Id)
                .OrderByDescending(r => r.Tarih)
                .ThenBy(r => r.BaslangicSaati)
                .ToListAsync();

            ViewBag.AsistanAdi = asistan.Ad + " " + asistan.Soyad;
            return View(randevular);
        }

        public async Task<IActionResult> HocaSec()
        {
            var hocalar = await _db.OgretimUyeleri
                .Include(o => o.Bolum)
                .Include(o => o.Musaitlikler.Where(m => m.Aktif))
                .Where(o => o.Musaitlikler.Any(m => m.Aktif))
                .OrderBy(o => o.Ad)
                .ToListAsync();
            return View(hocalar);
        }

        public async Task<IActionResult> Olustur(int id)
        {
            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Musaitlikler.Where(m => m.Aktif))
                .FirstOrDefaultAsync(o => o.Id == id);
            if (hoca == null) return NotFound();

            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null)
            {
                TempData["Error"] = "Hesabiniz bir asistan ile eslesmemis.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Hoca = hoca;
            ViewBag.Asistan = asistan;
            ViewBag.Musaitlikler = hoca.Musaitlikler.OrderBy(m => m.Gun).ThenBy(m => m.BaslangicSaati).ToList();

            return View(new Randevu
            {
                OgretimUyesiId = hoca.Id,
                AsistanId = asistan.Id,
                Tarih = DateTime.Today.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Randevu model)
        {
            // Id'yi her zaman sifirla -- SQL Server otomatik atasin
            model.Id = 0;

            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null)
            {
                TempData["Error"] = "Hesabiniz bir asistan ile eslesmemis.";
                return RedirectToAction(nameof(Index));
            }
            model.AsistanId = asistan.Id;
            model.OlusturmaTarihi = DateTime.Now;
            model.Durum = RandevuDurumu.Beklemede;

            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (model.Tarih.Date < DateTime.Today)
                ModelState.AddModelError("Tarih", "Gecmis bir tarihe randevu olusturulamaz.");

            var cakisma = await _db.Randevular.AnyAsync(r =>
                r.OgretimUyesiId == model.OgretimUyesiId &&
                r.Tarih == model.Tarih &&
                r.BaslangicSaati == model.BaslangicSaati &&
                r.Durum != RandevuDurumu.Iptal);
            if (cakisma)
                ModelState.AddModelError("", "Bu saat icin baska bir randevu zaten alinmis.");

            var olunanGun = model.Tarih.DayOfWeek;
            var slotVarMi = await _db.Musaitlikler.AnyAsync(m =>
                m.OgretimUyesiId == model.OgretimUyesiId &&
                m.Gun == olunanGun &&
                m.Aktif &&
                m.BaslangicSaati <= model.BaslangicSaati &&
                m.BitisSaati >= model.BitisSaati);
            if (!slotVarMi)
                ModelState.AddModelError("", "Secilen tarih ve saat hocanin musaitlik slotuna uymuyor.");

            if (ModelState.IsValid)
            {
                _db.Randevular.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Randevu olusturuldu. Onay icin bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Musaitlikler.Where(m => m.Aktif))
                .FirstOrDefaultAsync(o => o.Id == model.OgretimUyesiId);
            ViewBag.Hoca = hoca;
            ViewBag.Asistan = asistan;
            ViewBag.Musaitlikler = hoca?.Musaitlikler.OrderBy(m => m.Gun).ThenBy(m => m.BaslangicSaati).ToList();
            return View(model);
        }

        public async Task<IActionResult> Duzenle(int? id)
        {
            if (id == null) return NotFound();
            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null) return Forbid();

            var r = await _db.Randevular
                .Include(x => x.OgretimUyesi)
                    .ThenInclude(o => o!.Musaitlikler)
                .FirstOrDefaultAsync(x => x.Id == id && x.AsistanId == asistan.Id);
            if (r == null) return NotFound();
            if (r.Durum != RandevuDurumu.Beklemede)
            {
                TempData["Error"] = "Sadece beklemedeki randevular duzenlenebilir.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Hoca = r.OgretimUyesi;
            ViewBag.Asistan = asistan;
            ViewBag.Musaitlikler = r.OgretimUyesi?.Musaitlikler.Where(m => m.Aktif).OrderBy(m => m.Gun).ThenBy(m => m.BaslangicSaati).ToList();
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Randevu model)
        {
            if (id != model.Id) return NotFound();
            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null || model.AsistanId != asistan.Id) return Forbid();

            var mevcut = await _db.Randevular.FirstOrDefaultAsync(r => r.Id == id && r.AsistanId == asistan.Id);
            if (mevcut == null) return NotFound();
            if (mevcut.Durum != RandevuDurumu.Beklemede)
            {
                TempData["Error"] = "Sadece beklemedeki randevular duzenlenebilir.";
                return RedirectToAction(nameof(Index));
            }

            if (model.BitisSaati <= model.BaslangicSaati)
                ModelState.AddModelError("BitisSaati", "Bitis saati baslangictan sonra olmalidir.");

            if (model.Tarih.Date < DateTime.Today)
                ModelState.AddModelError("Tarih", "Gecmis bir tarihe randevu konulamaz.");

            if (ModelState.IsValid)
            {
                mevcut.Tarih = model.Tarih;
                mevcut.BaslangicSaati = model.BaslangicSaati;
                mevcut.BitisSaati = model.BitisSaati;
                mevcut.Konu = model.Konu;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Randevu guncellendi.";
                return RedirectToAction(nameof(Index));
            }

            var hoca = await _db.OgretimUyeleri
                .Include(o => o.Musaitlikler.Where(m => m.Aktif))
                .FirstOrDefaultAsync(o => o.Id == mevcut.OgretimUyesiId);
            ViewBag.Hoca = hoca;
            ViewBag.Asistan = asistan;
            ViewBag.Musaitlikler = hoca?.Musaitlikler.OrderBy(m => m.Gun).ThenBy(m => m.BaslangicSaati).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id)
        {
            var asistan = await GetCurrentAsistanAsync();
            if (asistan == null) return Forbid();

            var r = await _db.Randevular.FirstOrDefaultAsync(x => x.Id == id && x.AsistanId == asistan.Id);
            if (r == null) return NotFound();
            if (r.Durum == RandevuDurumu.Tamamlandi)
            {
                TempData["Error"] = "Tamamlanmis randevu iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }
            r.Durum = RandevuDurumu.Iptal;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Asistan?> GetCurrentAsistanAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null) return null;
            return await _db.Asistanlar.FirstOrDefaultAsync(a => a.Email == user.Email);
        }
    }
}
