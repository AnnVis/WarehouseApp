using NetFrameworkDemo.Models;

namespace NetFrameworkDemo.Repositories
{
    public interface IUserRepository
    {
        User FindByUsername(string username);
    }
}