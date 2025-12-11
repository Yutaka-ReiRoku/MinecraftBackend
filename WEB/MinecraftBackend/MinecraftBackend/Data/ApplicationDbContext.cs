using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;

namespace MinecraftBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- CHỈ GIỮ LẠI 5 BẢNG CỐT LÕI NÀY ---
        
        // 1. Bảng Users
        public DbSet<User> Users { get; set; }

        // 2. Bảng PlayerProfiles
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }

        // 3. Bảng ShopItems (Sản phẩm trong Shop)
        public DbSet<ShopItem> ShopItems { get; set; }

        // 4. Bảng Inventories (Kho đồ người chơi)
        public DbSet<GameInventory> Inventories { get; set; }

        // 5. Bảng Transactions (Lịch sử giao dịch)
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập ràng buộc Unique cho Username và Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
        }
    }
}