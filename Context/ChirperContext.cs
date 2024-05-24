namespace final_project_back_end_akostryba;
using Microsoft.EntityFrameworkCore;



public class ChirperContext : DbContext
{
    public DbSet<User> Users { get; set; } // Replace with your actual model
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=ChirperDb.db");
    }
}
