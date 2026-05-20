using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using WarehouseApp.Data;
using WarehouseApp.Models;
using WarehouseApp.Services;
using System.Security.Claims;
using System.Web;
using StorageManager.Models;
using StorageManager.Services;

namespace WarehouseApp.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly TokenService _tokenService = new TokenService();

        // POST: api/account/register
        [AllowAnonymous, HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _db.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (exists) return Content(HttpStatusCode.Conflict, "Username or email already in use.");

            PasswordHasher.CreatePasswordHash(model.Password, out var hash, out var salt);

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _db.Users.Add(user);

            // assign default role "User" if exists, otherwise create it
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (role == null)
            {
                role = new Role { Name = "User" };
                _db.Roles.Add(role);
                await _db.SaveChangesAsync(); // ensure role has id
            }
            _db.UserRoles.Add(new UserRole { User = user, Role = role });

            await _db.SaveChangesAsync();

            return Ok(new { message = "Registered" });
        }

        // POST: api/account/login
        [AllowAnonymous, HttpPost, Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _db.Users.Include(u => u.UserRoles.Select(ur => ur.Role))
                .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

            if (user == null) return Unauthorized();

            var valid = PasswordHasher.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt);
            if (!valid) return Unauthorized();

            var token = _tokenService.GenerateJwt(user, TimeSpan.FromHours(8));

            return Ok(new { token, expires_in = 8 * 3600 });
        }

        // GET: api/account/me
        [Authorize, HttpGet, Route("me")]
        public async Task<IHttpActionResult> Me()
        {
            var principal = HttpContext.Current.User as ClaimsPrincipal;
            var idClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var id)) return Unauthorized();

            var user = await _db.Users.Include(u => u.UserRoles.Select(ur => ur.Role)).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Phone,
                user.Address,
                Roles = user.UserRoles.Select(ur => ur.Role.Name)
            });
        }

        // POST: api/account/assign-role
        // Only allowed to users in "Admin" role
        [Authorize, HttpPost, Route("assign-role")]
        public async Task<IHttpActionResult> AssignRole(int userId, string roleName)
        {
            var principal = HttpContext.Current.User as ClaimsPrincipal;
            if (!principal.IsInRole("Admin")) return Content(HttpStatusCode.Forbidden, "Admin role required.");

            var user = await _db.Users.Include(u => u.UserRoles.Select(ur => ur.Role)).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                role = new Role { Name = roleName };
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();
            }

            if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
            {
                _db.UserRoles.Add(new UserRole { User = user, Role = role });
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = "Role assigned" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}