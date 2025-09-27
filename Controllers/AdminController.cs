using DeliStarter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliStarter.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // === Delete User ===
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Redirect("/admin?error=User not found");
            }

            // don’t allow admin to delete themselves
            if (user.Email == "admin@deli.com")
            {
                return Redirect("/admin?error=Cannot delete main admin");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Redirect("/admin?success=User deleted");
            }
            else
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                return Redirect($"/admin?error={Uri.EscapeDataString(error)}");
            }
        }

        // === Update User Role ===
[HttpPost("updaterole/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateRole(string id, string role)
{
    var user = await _userManager.FindByIdAsync(id);
    if (user == null)
    {
        return Redirect("/admin?error=User not found");
    }

    // Remove current roles
    var currentRoles = await _userManager.GetRolesAsync(user);
    await _userManager.RemoveFromRolesAsync(user, currentRoles);

    // Assign new role
    var result = await _userManager.AddToRoleAsync(user, role);

    if (!result.Succeeded)
    {
        var error = string.Join("; ", result.Errors.Select(e => e.Description));
        return Redirect($"/admin?error={Uri.EscapeDataString(error)}");
    }

    // === Sync with Vendors table ===
    var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);

    if (role == "Vendor")
    {
        // If switching to Vendor, ensure vendor entry exists
        if (vendor == null)
        {
            _db.Vendors.Add(new Vendor
            {
                ApplicationUserId = user.Id,
                DisplayName = user.UserName ?? user.Email ?? "Unnamed Vendor",
                Bio = "",
                CreatedAt = DateTime.UtcNow,
                IsApproved = true
            });
            await _db.SaveChangesAsync();
        }
    }
    else
    {
        // If switching away from Vendor, remove from Vendors table
        if (vendor != null)
        {
            _db.Vendors.Remove(vendor);
            await _db.SaveChangesAsync();
        }
    }

    return Redirect("/admin?success=Role updated");
}


        // === Search Users ===
        [HttpGet("search")]
        public IActionResult Search(string query = "")
        {
            // Search is handled on the client/Blazor side for display.
            // We redirect with the query so the Blazor page can pick it up.
            return Redirect($"/admin?search={Uri.EscapeDataString(query ?? "")}");
        }

        // === Toggle Vendor Approval (Approve/Reject) ===
        [HttpPost("vendor/toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVendor(string id)
        {
            var vendor = await _db.Vendors.FindAsync(id);
            if (vendor == null)
                return Redirect("/admin/vendors?error=Vendor not found");

            vendor.IsApproved = !vendor.IsApproved;
            await _db.SaveChangesAsync();

            return Redirect("/admin/vendors?success=Vendor updated");
        }

        // === Delete Vendor ===
        [HttpPost("vendor/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVendor(string id)
        {
            var vendor = await _db.Vendors
                .Include(v => v.Adverts) // include for clarity — cascade delete configured in model
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendor == null)
                return Redirect("/admin/vendors?error=Vendor not found");

            // Optionally: remove "Vendor" role from the linked ApplicationUser (if any)
            if (!string.IsNullOrEmpty(vendor.ApplicationUserId))
            {
                var appUser = await _userManager.FindByIdAsync(vendor.ApplicationUserId);
                if (appUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(appUser);
                    if (roles.Contains("Vendor"))
                    {
                        await _userManager.RemoveFromRoleAsync(appUser, "Vendor");
                    }
                }
            }

            // Remove vendor — Adverts cascade if configured
            _db.Vendors.Remove(vendor);
            await _db.SaveChangesAsync();

            return Redirect("/admin/vendors?success=Vendor deleted");
        }

        // === Delete Advert ===
        [HttpPost("advert/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdvert(int id)
        {
            var advert = await _db.Adverts.FindAsync(id);
            if (advert == null)
                return Redirect("/admin/adverts?error=Advert not found");

            _db.Adverts.Remove(advert);
            await _db.SaveChangesAsync();

            return Redirect("/admin/adverts?success=Advert deleted");
        }


        // === Update (Edit) User ===
        [HttpPost("update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, string email)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Redirect("/admin?error=User not found");
            }

            user.Email = email;
            user.UserName = email; // keep username same as email

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Redirect("/admin?success=User updated");
            }
            else
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                return Redirect($"/admin/edit/{id}?error={Uri.EscapeDataString(error)}");
            }
        }
    }
}
