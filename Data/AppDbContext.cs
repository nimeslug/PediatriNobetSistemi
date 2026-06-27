using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Bolum> Bolumler => Set<Bolum>();
        public DbSet<Asistan> Asistanlar => Set<Asistan>();
        public DbSet<OgretimUyesi> OgretimUyeleri => Set<OgretimUyesi>();
        public DbSet<Nobet> Nobetler => Set<Nobet>();
        public DbSet<Musaitlik> Musaitlikler => Set<Musaitlik>();
        public DbSet<Randevu> Randevular => Set<Randevu>();
        public DbSet<AcilDurumHaberi> AcilDurumHaberleri => Set<AcilDurumHaberi>();
        public DbSet<MailLog> MailLoglari => Set<MailLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bolum - SorumluOgretimUyesi (1-1 opsiyonel)
            modelBuilder.Entity<Bolum>()
                .HasOne(b => b.SorumluOgretimUyesi)
                .WithOne()
                .HasForeignKey<Bolum>(b => b.SorumluOgretimUyesiId)
                .OnDelete(DeleteBehavior.SetNull);

            // OgretimUyesi - Bolum (n-1)
            modelBuilder.Entity<OgretimUyesi>()
                .HasOne(o => o.Bolum)
                .WithMany()
                .HasForeignKey(o => o.BolumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Asistan - Bolum (n-1)
            modelBuilder.Entity<Asistan>()
                .HasOne(a => a.Bolum)
                .WithMany(b => b.Asistanlar)
                .HasForeignKey(a => a.BolumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Nobet - Asistan
            modelBuilder.Entity<Nobet>()
                .HasOne(n => n.Asistan)
                .WithMany(a => a.Nobetler)
                .HasForeignKey(n => n.AsistanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Nobet - Bolum
            modelBuilder.Entity<Nobet>()
                .HasOne(n => n.Bolum)
                .WithMany(b => b.Nobetler)
                .HasForeignKey(n => n.BolumId)
                .OnDelete(DeleteBehavior.Restrict);

            // Aynı asistan aynı gün aynı saatte iki nöbet yazamasın
            modelBuilder.Entity<Nobet>()
                .HasIndex(n => new { n.AsistanId, n.Tarih, n.BaslangicSaati })
                .IsUnique();

            // Aynı bölümde aynı gün aynı saatte iki asistan olmasın
            modelBuilder.Entity<Nobet>()
                .HasIndex(n => new { n.BolumId, n.Tarih, n.BaslangicSaati })
                .IsUnique();

            // Musaitlik - OgretimUyesi
            modelBuilder.Entity<Musaitlik>()
                .HasOne(m => m.OgretimUyesi)
                .WithMany(o => o.Musaitlikler)
                .HasForeignKey(m => m.OgretimUyesiId)
                .OnDelete(DeleteBehavior.Cascade);

            // Randevu - Asistan
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Asistan)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AsistanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Randevu - OgretimUyesi
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.OgretimUyesi)
                .WithMany(o => o.Randevular)
                .HasForeignKey(r => r.OgretimUyesiId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu - Musaitlik (opsiyonel)
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Musaitlik)
                .WithMany()
                .HasForeignKey(r => r.MusaitlikId)
                .OnDelete(DeleteBehavior.SetNull);

            // AcilDurumHaberi - Bolum (opsiyonel)
            modelBuilder.Entity<AcilDurumHaberi>()
                .HasOne(a => a.Bolum)
                .WithMany()
                .HasForeignKey(a => a.BolumId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}