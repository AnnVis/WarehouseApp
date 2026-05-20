using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using System.Security.Claims;
using StorageManager.Data;
using StorageManager.Models;

namespace StorageManager.Controllers
{
    [RoutePrefix("api/items")]
    public class ItemsController : ApiController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: api/items
        [Authorize, HttpGet, Route("")]
        public async Task<IHttpActionResult> GetAll()
        {
            var items = await _db.Items
                .Include(i => i.AddedBy)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    RoomNumber = i.RoomNumber,
                    Section = i.Section,
                    AddedByName = i.AddedByName ?? i.AddedBy.Username,
                    AddedById = i.AddedById,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/items/{id}
        [Authorize, HttpGet, Route("{id:int}", Name = "GetItemById")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var item = await _db.Items
                .Include(i => i.AddedBy)
                .Where(i => i.Id == id)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    RoomNumber = i.RoomNumber,
                    Section = i.Section,
                    AddedByName = i.AddedByName ?? i.AddedBy.Username,
                    AddedById = i.AddedById,
                    CreatedAt = i.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/items
        // Any authenticated user may add an item; controller will record who added it.
        [Authorize, HttpPost, Route("")]
        public async Task<IHttpActionResult> Create(CreateItemRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var codeExists = await _db.Items.AnyAsync(i => i.Code == model.Code);
                if (codeExists) return Content(HttpStatusCode.Conflict, "Item code already in use.");
            }

            // determine AddedBy
            string addedByName = model.AddedByName;
            int? addedById = model.AddedById;

            if (addedById.HasValue)
            {
                var user = await _db.Users.FindAsync(addedById.Value);
                if (user != null)
                {
                    addedByName = user.Username;
                }
            }

            if (string.IsNullOrWhiteSpace(addedByName))
            {
                var principal = HttpContext.Current.User as ClaimsPrincipal;
                addedByName = principal?.FindFirst(ClaimTypes.Name)?.Value;
            }

            if (string.IsNullOrWhiteSpace(addedByName))
            {
                addedByName = "Unknown";
            }

            var item = new Item
            {
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                Quantity = model.Quantity,
                RoomNumber = model.RoomNumber,
                Section = model.Section,
                AddedByName = addedByName,
                AddedById = addedById
            };

            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            var dto = new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Description = item.Description,
                Quantity = item.Quantity,
                RoomNumber = item.RoomNumber,
                Section = item.Section,
                AddedById = item.AddedById,
                AddedByName = item.AddedByName,
                CreatedAt = item.CreatedAt
            };

            return CreatedAtRoute("GetItemById", new { id = item.Id }, dto);
        }

        // PUT: api/items/{id}
        // Restrict update/delete to Admins
        [Authorize(Roles = "Admin"), HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> Update(int id, UpdateItemRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.Equals(item.Code, model.Code, System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(model.Code))
            {
                var codeTaken = await _db.Items.AnyAsync(i => i.Code == model.Code && i.Id != id);
                if (codeTaken) return Content(HttpStatusCode.Conflict, "Item code already in use.");
            }

            item.Name = model.Name;
            item.Code = model.Code;
            item.Description = model.Description;
            item.Quantity = model.Quantity;
            item.RoomNumber = model.RoomNumber;
            item.Section = model.Section;

            if (model.AddedById.HasValue)
            {
                var user = await _db.Users.FindAsync(model.AddedById.Value);
                if (user != null)
                {
                    item.AddedById = user.Id;
                    item.AddedByName = user.Username;
                }
                else
                {
                    item.AddedById = null;
                    item.AddedByName = model.AddedByName;
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.AddedByName))
            {
                item.AddedByName = model.AddedByName;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Updated" });
        }

        // DELETE: api/items/{id}
        [Authorize(Roles = "Admin"), HttpDelete, Route("{id:int}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();

            _db.Items.Remove(item);
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