using System.Web.Mvc;

namespace NetFrameworkDemo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.User = User.Identity.Name;
            return View();
        }
    }
}