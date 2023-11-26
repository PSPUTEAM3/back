using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebApplication3;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<ApplicationUser> users { get; set; }
    public DbSet<InvalidToken> InvalidTokens { get; set; }
    //public DbSet<Post> post { get; set; }
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<InvalidToken>().HasKey(j => j.Id); // Замените 'Id' на имя свойства, которое должно быть первичным ключом
    //}

}