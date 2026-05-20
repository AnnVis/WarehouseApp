using StorageManager.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using StorageManager.Models;

namespace StorageManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Ensure you have a connection string named "DefaultConnection" in Web.config
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Optional: configure unique index via fluent api or migrations
            base.OnModelCreating(modelBuilder);
        }
    }
}