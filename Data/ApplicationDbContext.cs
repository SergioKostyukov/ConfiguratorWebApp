using Microsoft.EntityFrameworkCore;
using ConfiguratorWebApp.Models.Entites;

namespace ConfiguratorWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Configuration> Configurations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}
