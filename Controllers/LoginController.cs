using DeliStarter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DeliStarter.Controllers
{
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("signin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                        return Redirect("/admin");

                    if (roles.Contains("Vendor"))
                        return Redirect("/vendor/dashboard");

                    // Default: normal user
                    return Redirect("/");
                }

                return Redirect("/"); // fallback if no user found
            }

            Console.WriteLine("❌ Invalid login attempt");
            return Redirect("/login?error=1"); // redirect back to login page
        }
    }
}
