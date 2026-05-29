using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using StorageManager.Data;
using StorageManager.Repositories;
using StorageManager.Repositories.Interfaces;
using StorageManager.Services;
using StorageManager.Services.Interfaces;

namespace StorageManager.App_Start
{
    public static class AutofacConfig
    {
        public static void Register(
            HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            // Controllers
            builder.RegisterApiControllers(
                Assembly.GetExecutingAssembly());

            // DbContext
            builder.RegisterType<ApplicationDbContext>()
                .InstancePerRequest();

            // Repositories
            builder.RegisterType<UserRepository>()
                .As<IUserRepository>()
                .InstancePerRequest();

            builder.RegisterType<ItemRepository>()
                .As<IItemRepository>()
                .InstancePerRequest();

            builder.RegisterType<RoleRepository>()
                .As<IRoleRepository>()
                .InstancePerRequest();

            // Services
            builder.RegisterType<UserService>()
                .As<IUserService>()
                .InstancePerRequest();

            builder.RegisterType<ItemService>()
                .As<IItemService>()
                .InstancePerRequest();

            builder.RegisterType<AccountService>()
                .As<IAccountService>()
                .InstancePerRequest();

            var container = builder.Build();

            config.DependencyResolver =
                new AutofacWebApiDependencyResolver(
                    container);
        }
    }
}