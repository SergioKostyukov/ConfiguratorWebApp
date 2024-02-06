using Microsoft.EntityFrameworkCore;
using ConfiguratorWebApp.Models.Entities;

namespace ConfiguratorWebApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Configuration> Configurations { get; set; }
    }
}
