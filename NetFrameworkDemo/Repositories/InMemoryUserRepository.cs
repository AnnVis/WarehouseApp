using System.Collections.Generic;
using System.Linq;
using NetFrameworkDemo.Models;
using NetFrameworkDemo.Helpers;

namespace NetFrameworkDemo.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public InMemoryUserRepository()
        {
            // Seed user: username = admin, password = P@ssw0rd
            _users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = PasswordHasher.CreateHash("P@ssw0rd"),
                    Role = "Admin"
                }
            };
        }

        public User FindByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}