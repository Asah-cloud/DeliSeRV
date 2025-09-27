using DeliStarter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliStarter.Controllers
{
    [Route("vendor")]
    [Authorize(Roles = "Vendor")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // === Create Advert ===
        [HttpPost("adverts/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdvert(string title, string description, decimal price)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Redirect("/vendor/dashboard?error=User not found");

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return Redirect("/vendor/dashboard?error=Vendor profile not found");

            var advert = new Advert
            {
                VendorId = vendor.Id,
                Title = title,
                Description = description,
                Price = price,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Adverts.Add(advert);
            await _db.SaveChangesAsync();

            return Redirect("/vendor/dashboard?success=Advert created");
        }

        // === Update Advert ===
        [HttpPost("adverts/update/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdvert(int id, string title, string description, decimal price, bool isActive)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Redirect("/vendor/dashboard?error=User not found");

            var advert = await _db.Adverts.FirstOrDefaultAsync(a => a.Id == id);
            if (advert == null)
                return Redirect("/vendor/dashboard?error=Advert not found");

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null || advert.VendorId != vendor.Id)
                return Redirect("/vendor/dashboard?error=Not authorized");

            advert.Title = title;
            advert.Description = description;
            advert.Price = price;
            advert.IsActive = isActive;

            await _db.SaveChangesAsync();

            return Redirect("/vendor/dashboard?success=Advert updated");
        }
        // === Update Vendor Profile ===
[HttpPost("profile/update")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateProfile(string displayName, string bio)
{
    // find the currently logged in user
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Redirect("/vendor/profile?error=User not found");

    // find vendor profile
    var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
    if (vendor == null)
        return Redirect("/vendor/profile?error=Vendor profile not found");

    // update fields
    vendor.DisplayName = displayName;
    vendor.Bio = bio;

    await _db.SaveChangesAsync();

    return Redirect("/vendor/profile?success=Profile updated");
}


        // === Delete Advert ===
        [HttpPost("adverts/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdvert(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Redirect("/vendor/dashboard?error=User not found");

            var advert = await _db.Adverts.FirstOrDefaultAsync(a => a.Id == id);
            if (advert == null)
                return Redirect("/vendor/dashboard?error=Advert not found");

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null || advert.VendorId != vendor.Id)
                return Redirect("/vendor/dashboard?error=Not authorized");

            _db.Adverts.Remove(advert);
            await _db.SaveChangesAsync();

            return Redirect("/vendor/dashboard?success=Advert deleted");
        }
    }
}
