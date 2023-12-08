using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Portal.Controllers
{
    public class AuthController : Controller
    {
        readonly string UserPassword = string.Empty;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            string? FromAppSettings = _configuration.GetSection("AppSettings:UserPassword").Value;
            if (FromAppSettings != null)
            {
                UserPassword = FromAppSettings;
            }
            else
            {
                UserPassword = "Qwert@123";
            }
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Names", "List");
            }
            return View();
        }
        //login
        [HttpPost]
        public async Task<IActionResult> Login([Required] string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "كلمة المرور مطلوبة");
            }
            if (ModelState.IsValid)
            {
                if (Password == UserPassword)
                {

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "Hamad"),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await _httpContextAccessor.HttpContext.SignInAsync(
                  CookieAuthenticationDefaults.AuthenticationScheme,
                  new ClaimsPrincipal(claimsIdentity),
                  authProperties);
                    return RedirectToAction("Names", "List");
                }
                else
                {
                    ModelState.AddModelError("Password", "كلمة المرور غير صحيحة");
                }
            }
            return View("Index");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Index", "Auth");
        }
    }
}
