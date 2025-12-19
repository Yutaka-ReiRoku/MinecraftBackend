using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;
using System.Threading;

namespace MinecraftBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<GameInventory> Inventories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Monster> Monsters { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}