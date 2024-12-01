using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
namespace EFCore.Models;
public class EFContext : DbContext
{
    private const string connectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=JanSamoeil;Trusted_Connection=True;";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }
    public DbSet<WeatherData> WeatherData { get; set; } // Lägg till DbSet
}


