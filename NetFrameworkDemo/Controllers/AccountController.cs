using System.Web.Mvc;
using System.Web.Security;
using NetFrameworkDemo.Models;
using NetFrameworkDemo.Repositories;

namespace NetFrameworkDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _users;

        public AccountController()
        {
            // In production, inject a repository (DI). For now use in-memory seed.
            _users = new InMemoryUserRepository();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _users.FindByUsername(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            if (!Helpers.PasswordHasher.VerifyHash(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Successful login: set auth cookie
            FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}