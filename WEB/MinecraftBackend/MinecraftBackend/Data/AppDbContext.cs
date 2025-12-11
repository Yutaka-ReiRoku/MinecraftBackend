using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;

namespace MinecraftBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- 1. CORE & USER ---
        public DbSet<User> Users { get; set; }
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }
        public DbSet<PlayerActivityLog> PlayerActivityLogs { get; set; }
        public DbSet<PlayerMail> PlayerMails { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<PlayerAchievement> PlayerAchievements { get; set; }

        // --- 2. MASTER DATA (GAMEPLAY) ---
        public DbSet<Item> Items { get; set; }
        public DbSet<MonsterData> MonsterDatas { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<UpgradeRule> UpgradeRules { get; set; }
        public DbSet<CraftingRecipe> CraftingRecipes { get; set; }

        // --- 3. SHOP & ECONOMY ---
        public DbSet<ObjectBundle> ObjectBundles { get; set; }
        public DbSet<BundleEntry> BundleEntries { get; set; }
        public DbSet<ShopProduct> ShopProducts { get; set; }

        // --- 4. RUNTIME DATA (PLAYER STATE) ---
        public DbSet<GameInventory> GameInventories { get; set; }
        public DbSet<PlayerBuilding> PlayerBuildings { get; set; }
        public DbSet<PlayerQuestProgress> PlayerQuestProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CẤU HÌNH QUAN HỆ (RELATIONSHIPS) ---

            // 1. User - Profile (1-1)
            // Khi xóa User -> Xóa luôn Profile
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<PlayerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Bundle - BundleEntry (1-N)
            // Khi xóa Bundle -> Xóa hết các mục con trong bundle đó
            modelBuilder.Entity<ObjectBundleEntry>()
                .HasOne(e => e.ObjectBundle)
                .WithMany(b => b.Entries)
                .HasForeignKey(e => e.BundleId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. ShopProduct Configuration
            // Sản phẩm trong shop liên kết với 2 Bundle (Hàng & Giá)
            modelBuilder.Entity<ShopProduct>()
                .HasOne(s => s.ProductBundle)
                .WithMany()
                .HasForeignKey(s => s.ProductBundleID)
                .OnDelete(DeleteBehavior.Restrict); // Tránh xóa nhầm bundle gốc

            modelBuilder.Entity<ShopProduct>()
                .HasOne(s => s.PriceBundle)
                .WithMany()
                .HasForeignKey(s => s.PriceBundleID)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Inventory Configuration
            // Xóa Character -> Xóa Inventory
            modelBuilder.Entity<GameInventory>()
                .HasOne(i => i.User) // Map qua User (PlayerProfile.UserId == User.Id)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Xóa Item -> Xóa Inventory (Hoặc Restrict tùy logic, ở đây chọn Cascade để sạch DB)
            modelBuilder.Entity<GameInventory>()
                .HasOne(i => i.Item)
                .WithMany()
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Crafting Recipe
            modelBuilder.Entity<CraftingRecipe>()
                .HasOne(r => r.ObjectBundle) // Nguyên liệu
                .WithMany()
                .HasForeignKey(r => r.MaterialBundleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Player Building
            modelBuilder.Entity<PlayerBuilding>()
                .HasOne(b => b.Building)
                .WithMany()
                .HasForeignKey(b => b.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- CẤU HÌNH DỮ LIỆU MẶC ĐỊNH (INITIAL SEEDING) ---
            // Chỉ seed những dữ liệu "Cứng" không thể thiếu. 
            // Dữ liệu Item/Shop số lượng lớn sẽ được xử lý bởi AdminController (Seeder)
            
            // Tạo Item tiền tệ mặc định để tránh lỗi logic khi khởi chạy lần đầu
            modelBuilder.Entity<Item>().HasData(
                new Item 
                { 
                    ItemId = "RES_GOLD", 
                    Name = "Gold Coin", 
                    Type = "Currency", 
                    PriceGold = 1, 
                    ProductImage = "/images/resources/gold_ingot.png",
                    MaxStackSize = 99999,
                    Description = "Tiền tệ chính."
                },
                new Item 
                { 
                    ItemId = "RES_GEM", 
                    Name = "Gem", 
                    Type = "Currency", 
                    PriceGold = 100, 
                    ProductImage = "/images/resources/diamond.png",
                    MaxStackSize = 99999,
                    Description = "Tiền tệ cao cấp."
                },
                new Item 
                { 
                    ItemId = "RES_EXP", 
                    Name = "Experience Orb", 
                    Type = "Currency", 
                    PriceGold = 0, 
                    ProductImage = "/images/others/exp.png",
                    MaxStackSize = 99999,
                    Description = "Điểm kinh nghiệm."
                }
            );
        }
    }
}