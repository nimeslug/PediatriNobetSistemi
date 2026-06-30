@'
# Pediatri Nöbet Yönetim Sistemi

Tıp Fakültesi Pediatri Kliniği için geliştirilmiş asistan nöbet, randevu ve acil durum bildirim yönetim sistemi.

ASP.NET Core MVC tabanlı tam yığın web uygulaması — bölüm, asistan, öğretim üyesi, nöbet programı, randevu ve acil durum modüllerini bir arada sunar. ISE 309 Web Programlama dersi proje ödevi kapsamında geliştirilmiştir.

## Özellikler

### Kullanıcı Tarafı
- Pediatri bölümleri ve detay sayfaları (yatak doluluğu, sorumlu hekim, asistan listesi)
- Asistan ve öğretim üyesi profilleri
- FullCalendar tabanlı nöbet takvimi (günlük / haftalık / aylık görünüm, modal detay)
- Online randevu sistemi (hoca seç → müsait slot → randevu oluştur)
- Kendi randevularını görüntüleme, düzenleme, iptal etme
- Acil durum bildirim sayfası (öncelik düzeyine göre renklendirilmiş)

### Yönetim Paneli
- ASP.NET Identity tabanlı rol bazlı yetkilendirme (Admin / Asistan / Öğretim Üyesi)
- Bölüm, Asistan, Öğretim Üyesi, Nöbet, Müsaitlik, Randevu CRUD modülleri
- Nöbet çakışma validasyonu
- Acil durum haberi yayınlama + tüm ekibe otomatik mail gönderimi (MailKit)
- Mail gönderim log takibi (başarılı/başarısız her gönderim kayıt altında)

### Teknik Özellikler
- Code First yaklaşımı (Entity Framework Core)
- 9 entity + ASP.NET Identity tabloları
- Custom Tag Helper (zorunlu alan işareti otomatik ekleyici)
- HTML email template
- Responsive Bootstrap 5 tabanlı kurumsal lacivert tasarım

## Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| Veritabanı | SQL Server 2025 Developer Edition |
| ORM | Entity Framework Core 10 (Code First) |
| Kimlik Doğrulama | ASP.NET Identity |
| Frontend | Bootstrap 5, Poppins (Google Fonts) |
| Takvim | FullCalendar 6.1 |
| Mail | MailKit + MimeKit |

## Kurulum

### Gereksinimler
- .NET 10 SDK
- SQL Server 2022/2025 (LocalDB veya Developer Edition)
- Visual Studio 2022 veya VS Code (C# Dev Kit ile)

### Adımlar

1. Repoyu klonlayın:
```bash
git clone https://github.com/nimeslug/PediatriNobetSistemi.git
cd PediatriNobetSistemi
```

2. `appsettings.json` içindeki bağlantı dizesini kendi SQL Server instance'ınıza göre düzenleyin:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\YOUR_INSTANCE;Database=PediatriNobetDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

3. (Opsiyonel) Mail gönderimi için SMTP ayarlarını ekleyin:
```json
"MailSettings": {
  "SmtpServer": "smtp-relay.brevo.com",
  "SmtpPort": 587,
  "SenderName": "Pediatri Nobet Sistemi",
  "SenderEmail": "your-email@example.com",
  "Password": "your-smtp-key"
}
```

4. Bağımlılıkları yükleyin ve uygulamayı başlatın:
```bash
dotnet restore
dotnet build
dotnet run
```

Uygulama otomatik olarak veritabanı migration'larını çalıştırır ve seed data (admin kullanıcısı + roller) ekler.

5. Tarayıcıda açın: `http://localhost:5088`

### Varsayılan Admin Hesabı
- **E-posta:** `admin@pediatri.local`
- **Şifre:** `Admin.2026!`

## Proje Yapısı
PediatriNobetSistemi/
├── Areas/
│   └── Admin/                  # Yönetim paneli (Area)
│       ├── Controllers/
│       └── Views/
├── Controllers/                # Kullanıcı tarafı controller'lar
├── Data/
│   ├── AppDbContext.cs         # EF Core DbContext
│   └── SeedData.cs             # Roller + admin seed
├── Models/Entities/            # 9 entity sınıfı
├── Services/
│   └── MailService.cs          # MailKit ile SMTP servisi
├── TagHelpers/
│   └── RequiredLabelTagHelper.cs
├── Views/                      # Razor view'lar
└── wwwroot/                    # CSS, JS, kütüphaneler
## Veri Modeli

- **Bolum** — Pediatri bölümleri (Çocuk Acil, Yoğun Bakım, Hematoloji, Onkoloji)
- **Asistan** — Asistan hekim bilgileri ve bölüm atamaları
- **OgretimUyesi** — Öğretim üyesi profilleri
- **Nobet** — Asistan nöbet programı (asistan-bölüm-tarih-saat)
- **Musaitlik** — Öğretim üyesi randevu slotları (haftalık tekrar)
- **Randevu** — Asistan-öğretim üyesi randevuları (durum: Beklemede/Onaylandı/İptal/Tamamlandı)
- **AcilDurumHaberi** — Acil duyurular (öncelik düzeyi ile)
- **MailLog** — Mail gönderim kayıtları

## Modüller ve Erişim

| Modül | URL | Erişim |
|-------|-----|--------|
| Anasayfa | `/` | Herkes |
| Bölümler | `/Bolumler` | Herkes |
| Asistanlar | `/Asistanlar` | Herkes |
| Öğretim Üyeleri | `/OgretimUyeleri` | Herkes |
| Nöbet Takvimi | `/Takvim` | Herkes |
| Acil Durum Haberleri | `/AcilDurum` | Herkes |
| Randevularım | `/Randevu` | Giriş yapmış kullanıcı |
| Yönetim Paneli | `/Admin` | Yalnızca Admin |

## Geliştirici

**Gülsemin** ([@nimeslug](https://github.com/nimeslug))

## Lisans

Bu proje akademik amaçlı geliştirilmiştir. ISE 309 Web Programlama dersi proje ödevi.
'@ | Set-Content -Path README.md -Encoding UTF8
