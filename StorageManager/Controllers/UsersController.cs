using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Services;

namespace StorageManager.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: api/users
        [Authorize(Roles = "Admin"), HttpGet, Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var users = await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [Authorize(Roles = "Admin"), HttpGet, Route("{id:int}", Name = "GetUserById")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST: api/users
        [Authorize(Roles = "Admin"), HttpPost, Route("")]
        public async Task<IHttpActionResult> Create(CreateUserRequest model)
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
            await _db.SaveChangesAsync();

            // assign roles if provided
            if (model.Roles != null && model.Roles.Any())
            {
                foreach (var rn in model.Roles.Distinct())
                {
                    var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == rn) ?? new Role { Name = rn };
                    if (role.Id == 0) _db.Roles.Add(role);
                    _db.UserRoles.Add(new UserRole { User = user, Role = role });
                }
                await _db.SaveChangesAsync();
            }

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return CreatedAtRoute("GetUserById", new { id = user.Id }, dto);
        }

        // PUT: api/users/{id}
        [Authorize(Roles = "Admin"), HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> Update(int id, UpdateUserRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            // check uniqueness if changed
            if (!string.Equals(user.Username, model.Username, System.StringComparison.OrdinalIgnoreCase))
            {
                var usernameTaken = await _db.Users.AnyAsync(u => u.Username == model.Username && u.Id != id);
                if (usernameTaken) return Content(HttpStatusCode.Conflict, "Username already in use.");
                user.Username = model.Username;
            }

            if (!string.Equals(user.Email, model.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _db.Users.AnyAsync(u => u.Email == model.Email && u.Id != id);
                if (emailTaken) return Content(HttpStatusCode.Conflict, "Email already in use.");
                user.Email = model.Email;
            }

            user.Phone = model.Phone;
            user.Address = model.Address;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                PasswordHasher.CreatePasswordHash(model.Password, out var hash, out var salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            // update roles if provided (replace existing)
            if (model.Roles != null)
            {
                // remove existing
                var existingUserRoles = _db.UserRoles.Where(ur => ur.UserId == user.Id);
                _db.UserRoles.RemoveRange(existingUserRoles);

                foreach (var rn in model.Roles.Distinct())
                {
                    var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == rn) ?? new Role { Name = rn };
                    if (role.Id == 0) _db.Roles.Add(role);
                    _db.UserRoles.Add(new UserRole { User = user, Role = role });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Updated" });
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin"), HttpDelete, Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // remove related userroles first
            var urs = _db.UserRoles.Where(ur => ur.UserId == id);
            _db.UserRoles.RemoveRange(urs);
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Deleted" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}