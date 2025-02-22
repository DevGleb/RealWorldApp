using Microsoft.EntityFrameworkCore;
using RealWorldApp.Models;

namespace RealWorldApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
    }
}
