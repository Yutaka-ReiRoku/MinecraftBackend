using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;

namespace MinecraftBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 1. Bảng Users
        public DbSet<User> Users { get; set; }

        // 2. Bảng PlayerProfiles
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }

        // 3. Bảng ShopItems (Gộp Items & ShopProducts)
        public DbSet<ShopItem> ShopItems { get; set; }

        // 4. Bảng Inventory
        public DbSet<GameInventory> Inventories { get; set; }

        // 5. Bảng Transactions (Đổi tên từ PlayerActivityLogs -> Transactions)
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
        }
    }
}