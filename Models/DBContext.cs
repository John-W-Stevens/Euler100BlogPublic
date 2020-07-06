using System;
using Microsoft.EntityFrameworkCore;

namespace EulerBlog.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options) { }

        public DbSet<EulerPost> EulerPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleAssociation> ModuleAssociations { get; set; }

    }
}

