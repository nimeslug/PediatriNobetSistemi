using Microsoft.AspNetCore.Identity;
using PediatriNobetSistemi.Models.Entities;

namespace PediatriNobetSistemi.Data
{
    public static class SeedData
    {
        public const string AdminRole = "Admin";
        public const string AsistanRole = "Asistan";
        public const string OgretimUyesiRole = "OgretimUyesi";

        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Rolleri oluştur
            string[] roles = { AdminRole, AsistanRole, OgretimUyesiRole };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Varsayılan admin
            const string adminEmail = "admin@pediatri.local";
            const string adminPassword = "Admin.2026!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Ad = "Sistem",
                    Soyad = "Yöneticisi"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AdminRole);
                }
            }
        }
    }
}