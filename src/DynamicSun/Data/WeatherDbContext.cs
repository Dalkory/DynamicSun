using DynamicSun.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicSun.Data
{
    public class WeatherDbContext : DbContext
    {
        private const string DefaultConnectionString = "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=676767";

        public DbSet<WeatherData> WeatherData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(DefaultConnectionString);
            }
        }
    }
}
