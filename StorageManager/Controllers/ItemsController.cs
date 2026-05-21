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
    [RoutePrefix("api/items")]
    public class ItemsController : ApiController
    {
        private readonly IItemService _itemService;

        public ItemsController()
        {
            var db = new ApplicationDbContext();

            _itemService = new ItemService(db);
        }

        // GET: api/items
        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var items = await _itemService
                .GetAllAsync();

            return Ok(items);
        }

        // GET: api/items/{id}
        [Authorize]
        [HttpGet]
        [Route("{id:int}", Name = "GetItemById")]
        public async Task<IHttpActionResult> GetById(
            int id)
        {
            var item = await _itemService
                .GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST: api/items
        [Authorize]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(
            CreateItemRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var principal =
                    HttpContext.Current.User
                    as ClaimsPrincipal;

                var username = principal?
                    .FindFirst(ClaimTypes.Name)?
                    .Value;

                var item = await _itemService
                    .CreateAsync(model, username);

                return CreatedAtRoute(
                    "GetItemById",
                    new { id = item.Id },
                    item);
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
        }

        // PUT: api/items/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Update(
            int id,
            UpdateItemRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _itemService
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

        // DELETE: api/items/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(
            int id)
        {
            var deleted = await _itemService
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