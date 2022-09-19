using Microsoft.AspNetCore.Mvc;
using RelatedPersonsModule.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using RelatedPersonsModule.Models;
using System.Security.Claims;
using TransactionTypeConstructor.Helper;
using System.Text.Json;

namespace RelatedPersonsModule.Controllers
{
    public class AuthentificationController : Controller
    {
        private static IDB db;
        private static ILogger<AuthentificationController> log;

        public AuthentificationController(IDB _db, ILogger<AuthentificationController> _log)
        {
            db = _db;
            log = _log;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {

            db.Init(model);
            if (db.TestConnection() == 1)
            {

                int access = db.GetAccessLevel(model);
                if (access == -1)
                {
                    model.UserName = "bnk_" + model.UserName;
                    model.Password = Utils.CriptoPassword(model.Password);
                    db.Init(model);
                    access = db.GetAccessLevel(model);
                }
                log.LogInformation($"User {model.UserName} logging!");

                if (access == 1)
                {
                    await Authenticate(model, "Basic");
                    return RedirectToAction("Index", "Home");
                }
                else if (access == 0)
                {
                    ViewBag.LoginMessage = "Sizin bu servisdən istifadə etməyə icazəniz yoxdur!";
                }
                else
                {
                    ViewBag.LoginMessage = "Xəta baş verdi!";
                }
            }
            else
            {
                ViewBag.LoginMessage = "Sizin İstifadəçi adınız və ya Şifrəniz yalnışdır və ya xəta baş verdi!";
            }
            return View(model);

        }

        private async Task Authenticate(LoginModel user, string role)
        {

            var claims = new List<Claim>
            {
                new Claim("user", user.UserName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
            };

            var id = new ClaimsIdentity(claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            HttpContext.Session.SetString("user", JsonSerializer.Serialize(user));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

    }
}
