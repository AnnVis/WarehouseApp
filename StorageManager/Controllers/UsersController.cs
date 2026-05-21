using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Services;
using StorageManager.Services.Interfaces;

namespace StorageManager.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUserService _userService;

        public UsersController()
        {
            var db = new ApplicationDbContext();

            _userService = new UserService(db);
        }

        // GET: api/users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("{id:int}", Name = "GetUserById")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/users
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(CreateUserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.CreateAsync(model);

                return CreatedAtRoute(
                    "GetUserById",
                    new { id = user.Id },
                    user);
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
        }

        // PUT: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Update(
            int id,
            UpdateUserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _userService
                    .UpdateAsync(id, model);

                if (!updated)
                    return NotFound();

                return Ok(new
                {
                    message = "Updated"
                });
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var deleted = await _userService
                .DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new
            {
                message = "Deleted"
            });
        }
    }
}