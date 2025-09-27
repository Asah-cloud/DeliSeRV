using DeliStarter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliStarter.Controllers
{
    [Route("register")]
    public class RegisterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                return Redirect("/register?error=Passwords do not match");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User"); // default role
                return Redirect("/login"); // after register go to login
            }
            else
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                return Redirect($"/register?error={Uri.EscapeDataString(error)}");
            }
        }
    }
}
