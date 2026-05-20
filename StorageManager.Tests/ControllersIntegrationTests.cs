using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http.Results;
using StorageManager.Data;
using StorageManager.Controllers;
using StorageManager.Models;

namespace StorageManager.Tests
{
    [TestClass]
    public class ControllersIntegrationTests
    {
        [TestInitialize]
        public void Init()
        {
            // clean DB before each test run to keep tests isolated
            using (var db = new ApplicationDbContext())
            {
                db.UserRoles.RemoveRange(db.UserRoles);
                db.Items.RemoveRange(db.Items);
                db.Roles.RemoveRange(db.Roles);
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();
            }
        }

        [TestMethod]
        public async Task User_Add_GetAll_GetById_Workflow()
        {
            var usersController = new UsersController();

            var createReq = new CreateUserRequest
            {
                Username = "testuser1",
                Email = "testuser1@example.com",
                Password = "P@ssw0rd",
                Phone = "123",
                Address = "Room 101",
                Roles = new System.Collections.Generic.List<string> { "User" }
            };

            // Create user
            var createResult = await usersController.Create(createReq);
            var createdAt = createResult as CreatedAtRouteNegotiatedContentResult<UserDto>;
            Assert.IsNotNull(createdAt, "Create should return CreatedAtRoute result");
            var createdUser = createdAt.Content;
            Assert.IsTrue(createdUser.Id > 0, "Created user must have Id");

            // Get all users
            var allResult = await usersController.GetAll();
            var okAll = allResult as OkNegotiatedContentResult<System.Collections.Generic.List<UserDto>>;
            Assert.IsNotNull(okAll, "GetAll should return Ok with list");
            Assert.IsTrue(okAll.Content.Any(u => u.Id == createdUser.Id), "Created user must be present in list");

            // Get by id
            var byIdResult = await usersController.GetById(createdUser.Id);
            var okById = byIdResult as OkNegotiatedContentResult<UserDto>;
            Assert.IsNotNull(okById, "GetById should return Ok with user");
            Assert.AreEqual(createdUser.Username, okById.Content.Username, "Username must match");
        }

        [TestMethod]
        public async Task Item_Add_GetAll_GetByUser_Workflow()
        {
            var usersController = new UsersController();
            var itemsController = new ItemsController();

            // create a user who will be the AddedBy
            var createUser = new CreateUserRequest
            {
                Username = "itemadder",
                Email = "itemadder@example.com",
                Password = "P@ssw0rd"
            };
            var createUserResult = await usersController.Create(createUser);
            var createdUserAt = createUserResult as CreatedAtRouteNegotiatedContentResult<UserDto>;
            Assert.IsNotNull(createdUserAt);
            var addedBy = createdUserAt.Content;

            // create an item, set AddedById
            var createItem = new CreateItemRequest
            {
                Name = "Pencil",
                Code = "PEN-001",
                Description = "HB pencil",
                Quantity = 10,
                RoomNumber = "101",
                Section = "Stationery",
                AddedById = addedBy.Id
            };

            var createItemResult = await itemsController.Create(createItem);
            var createdItemAt = createItemResult as CreatedAtRouteNegotiatedContentResult<ItemDto>;
            Assert.IsNotNull(createdItemAt, "Create item should return CreatedAtRoute");
            var createdItem = createdItemAt.Content;
            Assert.IsTrue(createdItem.Id > 0, "Created item must have Id");

            // get all items
            var allItemsResult = await itemsController.GetAll();
            var okAllItems = allItemsResult as OkNegotiatedContentResult<System.Collections.Generic.List<ItemDto>>;
            Assert.IsNotNull(okAllItems, "GetAll items should return Ok");
            Assert.IsTrue(okAllItems.Content.Any(i => i.Id == createdItem.Id), "Created item must be present in list");

            // get items by filtering client-side by AddedById (API endpoint for items-by-user not required to exist;
            // this verifies that items include AddedById and can be grouped client-side)
            var itemsByUser = okAllItems.Content.Where(i => i.AddedById == addedBy.Id).ToList();
            Assert.IsTrue(itemsByUser.Count >= 1, "There must be at least one item added by the created user");
            CollectionAssert.Contains(itemsByUser.Select(i => i.Id).ToList(), createdItem.Id, "Created item must be among items by user");
        }
    }
}