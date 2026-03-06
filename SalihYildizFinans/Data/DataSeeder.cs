using Microsoft.AspNetCore.Identity;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Data
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Ensure Roles Exist
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Ensure Default Admin User Exists
            var adminUser = await userManager.FindByEmailAsync("salih@yildiz.com");
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "salih",
                    Email = "salih@yildiz.com",
                    FirstName = "Salih",
                    LastName = "Yıldız",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!"); 
                if (result.Succeeded)
                {
                     await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure existing user has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Banks
            if (!context.Banks.Any())
            {
                context.Banks.AddRange(
                    new Bank { Name = "Garanti BBVA" },
                    new Bank { Name = "Enpara" },
                    new Bank { Name = "İş Bankası" },
                    new Bank { Name = "Akbank" },
                    new Bank { Name = "Yapı Kredi" },
                    new Bank { Name = "Ziraat Bankası" },
                    new Bank { Name = "Vakıfbank" },
                    new Bank { Name = "Nakit" }
                );
                await context.SaveChangesAsync();
            }

            if (adminUser != null)
            {
                // Fix Orphaned Transactions
                var orphanedTransactions = context.Transactions.Where(t => t.AppUserId == null).ToList();
                if (orphanedTransactions.Any())
                {
                    foreach (var t in orphanedTransactions)
                    {
                        t.AppUserId = adminUser.Id;
                        if(t.Date == DateTime.MinValue) t.Date = DateTime.Now;
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
