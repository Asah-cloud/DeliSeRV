using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeliStarter.Data
{
    public static class VendorAdvertSeeder
    {
        public static async Task SeedVendorsAndAdvertsAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            // Apply migrations (safe if already applied)
            await context.Database.MigrateAsync();

            // Check if we already have vendors
            if (context.Vendors.Any()) return;

            // 1️⃣ Create a test user for the vendor
            var vendorUser = await userManager.FindByEmailAsync("vendor@deli.com");
            if (vendorUser == null)
            {
                vendorUser = new ApplicationUser
                {
                    UserName = "vendor@deli.com",
                    Email = "vendor@deli.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(vendorUser, "Vendor123!");
                await userManager.AddToRoleAsync(vendorUser, "Vendor");
            }

            // 2️⃣ Create vendor profile
            var vendor = new Vendor
            {
                ApplicationUserId = vendorUser.Id,
                DisplayName = "Test Vendor",
                Bio = "We sell the best test products!"
            };
            context.Vendors.Add(vendor);
            await context.SaveChangesAsync();

            // 3️⃣ Add sample adverts
            var adverts = new List<Advert>
            {
                new Advert { VendorId = vendor.Id, Title = "Sample Item 1", Description = "First test advert", Price = 10.5m },
                new Advert { VendorId = vendor.Id, Title = "Sample Item 2", Description = "Second test advert", Price = 20.0m }
            };

            context.Adverts.AddRange(adverts);
            await context.SaveChangesAsync();
        }
    }
}
