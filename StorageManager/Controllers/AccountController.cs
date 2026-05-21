using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Services;
using StorageManager.Services.Interfaces;

namespace StorageManager.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private readonly IAccountService _accountService;

        public AccountController()
        {
            var db = new ApplicationDbContext();

            _accountService = new AccountService(db);
        }

        // POST: api/account/register
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IHttpActionResult> Register(
            RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _accountService
                    .RegisterAsync(model);

                return Ok(new
                {
                    message = "Registered"
                });
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
        }

        // POST: api/account/login
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(
            LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _accountService
                    .LoginAsync(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.Unauthorized,
                    ex.Message);
            }
        }

        // GET: api/account/me
        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IHttpActionResult> Me()
        {
            var principal =
                HttpContext.Current.User as ClaimsPrincipal;

            var idClaim = principal?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (!int.TryParse(idClaim, out var userId))
                return Unauthorized();

            var user = await _accountService
                .GetCurrentUserAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/account/assign-role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("assign-role")]
        public async Task<IHttpActionResult> AssignRole(
            int userId,
            string roleName)
        {
            try
            {
                await _accountService.AssignRoleAsync(
                    userId,
                    roleName);

                return Ok(new
                {
                    message = "Role assigned"
                });
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
        }
    }
}