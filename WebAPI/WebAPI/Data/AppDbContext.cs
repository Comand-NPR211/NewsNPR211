using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using WebAPI.Data.Entities;

namespace WebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NewsEntity> News { get; set; }

    }
}
