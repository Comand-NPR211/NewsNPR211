using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using WebAPI.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace WebAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>

    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NewsEntity> News { get; set; }
        public DbSet<Category> Categories { get; set; } // Додаємо категорії

    }
}
