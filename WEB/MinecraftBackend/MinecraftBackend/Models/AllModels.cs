using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinecraftBackend.Models
{
    // --- 1. CORE ENTITIES (NGƯỜI DÙNG & NHÂN VẬT) ---
    
    public class User
    {
        [Key] public string Id { get; set; } // GUID String
        [Required] public string UserName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active"; // Active, Banned
        public string Role { get; set; } = "Player"; // Player, Admin
        
        // Relationship 1-1 với Profile
        public virtual PlayerProfile? Profile { get; set; }
    }

    public class PlayerProfile
    {
        [Key, ForeignKey("User")] 
        public string UserId { get; set; }
        
        // Thông tin hiển thị
        public string DisplayName { get; set; } = "Steve";
        public string AvatarUrl { get; set; } = "/images/avatars/steve.png";
        
        // Kinh tế & Cấp độ
        public int Gold { get; set; } = 1000;
        public int Gem { get; set; } = 100;
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        
        // Sinh tồn & Chỉ số RPG
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Hunger { get; set; } = 20;
        public int MaxHunger { get; set; } = 20;
        public int Stamina { get; set; } = 100;
        
        // Trạng thái chơi & Hệ thống
        public string GameMode { get; set; } = "Survival"; // Survival, Creative, Hardcore
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public DateTime LastLogoutTime { get; set; } = DateTime.Now; // Cho AFK Farming
        
        // Daily Login
        public DateTime LastLoginDate { get; set; }
        public int LoginStreak { get; set; } = 0;
        public bool HasClaimedDaily { get; set; } = false;

        // Trang bị hiện tại (Visuals & Stats)
        public string? EquippedWeaponId { get; set; }
        public string? EquippedHelmetId { get; set; }
        public string? EquippedChestId { get; set; }
        public string? EquippedLegsId { get; set; }
        public string? EquippedBootsId { get; set; }
        public string? EquippedMountId { get; set; }
        
        // Trang phục (Vanity)
        public string? CostumeHeadId { get; set; }
        public string? CostumeBodyId { get; set; }

        public virtual User User { get; set; }
    }

    // --- 2. MASTER DATA (VẬT PHẨM & GAME OBJECTS) ---

    // Bảng cha chứa mọi thông tin chung của vật phẩm
    public class Item
    {
        [Key] public string ItemId { get; set; } // VD: "WEP_IRON_SWORD"
        [Required] public string Name { get; set; }
        public string Type { get; set; } // Weapon, Armor, Resource, Consumable, Vehicle, Bundle...
        public string Description { get; set; } = "No description.";
        public string ProductImage { get; set; } // URL ảnh (chuẩn cấu trúc folder)
        
        // Giá bán & Hiển thị Shop
        public int PriceGold { get; set; }
        public int PriceGem { get; set; }
        public bool IsShow { get; set; } = true;
        public DateTime ListedDate { get; set; } = DateTime.Now; // Cho tính năng "Hàng Mới"
        public int SoldCount { get; set; } = 0; // Cho tính năng "Best Seller"
        
        // Thuộc tính nâng cao
        public int MaxStackSize { get; set; } = 64;
        public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
        
        // Chỉ số RPG (Dùng chung để đơn giản hóa query)
        public int Attack { get; set; } = 0;
        public int Defense { get; set; } = 0;
        public int HealthRestore { get; set; } = 0;
        public int HungerRestore { get; set; } = 0;
        public int Durability { get; set; } = 100; // Độ bền tối đa
        public float SpeedBonus { get; set; } = 0; // Cho thú cưỡi
        public int EffectValue { get; set; } = 0; // Giá trị hiệu ứng chung
    }

    // Quái vật (Map với Item hoặc dùng riêng)
    public class MonsterData
    {
        [Key] public string MonsterId { get; set; } // VD: "MOB_ZOMBIE"
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public string? DropBundleId { get; set; } // Liên kết bảng Bundle
    }

    // --- 3. COMPLEX SYSTEMS (BUNDLE, CRAFT, BUILD) ---

    // Gói vật phẩm (Dùng cho Shop, Gacha, Drop, Recipe)
    public class ObjectBundle
    {
        [Key] public string BundleId { get; set; }
        public string BundleName { get; set; }
        public string Type { get; set; } // Loot, Cost, Product, Reward, Material
        public string? ImageUrl { get; set; }
        
        public virtual ICollection<BundleEntry> Entries { get; set; }
    }

    public class BundleEntry
    {
        [Key] public int Id { get; set; }
        [ForeignKey("ObjectBundle")] public string BundleId { get; set; }
        [ForeignKey("Item")] public string ItemId { get; set; }
        
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public double DropRate { get; set; } = 1.0; // Tỷ lệ rơi (0.0 - 1.0)

        public virtual ObjectBundle ObjectBundle { get; set; }
        public virtual Item Item { get; set; }
    }

    // Sản phẩm trong Shop (Liên kết Bundle Hàng và Bundle Giá)
    public class ShopProduct 
    {
        [Key] public string ProductID { get; set; } // VD: "SHOP_01"
        [ForeignKey("ProductBundle")] public string ProductBundleID { get; set; }
        [ForeignKey("PriceBundle")] public string PriceBundleID { get; set; }
        public string Status { get; set; } = "OnShelf";
        public DateTime ListedDate { get; set; } = DateTime.Now;

        public virtual ObjectBundle ProductBundle { get; set; }
        public virtual ObjectBundle PriceBundle { get; set; }
    }

    // Công thức chế tạo
    public class CraftingRecipe
    {
        [Key] public string RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Kết quả
        [ForeignKey("Item")] public string ResultItemId { get; set; }
        public int ResultCount { get; set; } = 1;
        public int CraftingTime { get; set; } = 5; // Giây

        // Nguyên liệu (Dùng Bundle để định nghĩa nhiều nguyên liệu)
        [ForeignKey("ObjectBundle")] public string MaterialBundleId { get; set; }

        public virtual Item Item { get; set; }
        public virtual ObjectBundle ObjectBundle { get; set; }
    }

    // Công trình xây dựng (Loại công trình)
    public class Building
    {
        [Key] public string BuildingId { get; set; } // VD: "BLD_HOUSE"
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int PriceGold { get; set; }
        public int DefenseBonus { get; set; } // Tăng thủ cho người chơi
        
        // Chi phí tài nguyên (Lưu JSON cho gọn: {"RES_WOOD": 50})
        public string ResourceCostJson { get; set; } = "{}";
    }

    // Nhiệm vụ
    public class Quest
    {
        [Key] public string QuestID { get; set; }
        public string QuestName { get; set; }
        public string Description { get; set; }
        public string QuestType { get; set; } // COLLECT, HUNT, CRAFT, BUY, SPEND
        public string TargetObjectID { get; set; } // ItemID hoặc MobID
        public int TargetAmount { get; set; }
        public string IconURL { get; set; }
        
        [ForeignKey("ObjectBundle")] public string RewardBundleID { get; set; }
        public virtual ObjectBundle RewardBundle { get; set; }
    }

    // Quy tắc nâng cấp (Upgrade)
    public class UpgradeRule
    {
        [Key] public string UpgradeRuleID { get; set; }
        public string BaseObjectID { get; set; }
        public string ResultObjectID { get; set; } // Item sau khi nâng cấp (nếu đổi ID)
        public string CostBundleID { get; set; } // Chi phí nguyên liệu
    }

    // --- 4. RUNTIME DATA (DỮ LIỆU NGƯỜI CHƠI) ---

    // Kho đồ của người chơi
    public class GameInventory
    {
        [Key] public string InventoryId { get; set; } // GUID
        [ForeignKey("User")] public string UserId { get; set; } // Map với PlayerProfile.UserId
        [ForeignKey("Item")] public string ItemId { get; set; }
        
        public int Quantity { get; set; }
        public int CurrentDurability { get; set; } // Độ bền hiện tại
        public int UpgradeLevel { get; set; } = 0; // Cấp cường hóa (+1, +2)
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // public virtual User User { get; set; } // Bỏ qua navigation này để tránh cycle nếu không cần thiết
        public virtual Item Item { get; set; }
    }

    // Công trình người chơi đã xây
    public class PlayerBuilding
    {
        [Key] public string Id { get; set; }
        public string UserId { get; set; }
        public string BuildingId { get; set; }
        public string Coordinates { get; set; } // Toạ độ X,Y
        public int CurrentDurability { get; set; } = 100;
        public DateTime BuiltAt { get; set; } = DateTime.Now;

        public virtual Building Building { get; set; }
    }

    // Tiến độ nhiệm vụ
    public class PlayerQuestProgress
    {
        [Key] public string Id { get; set; }
        public string CharacterID { get; set; } // UserId
        public string QuestID { get; set; }
        
        public int CurrentAmount { get; set; }
        public string Status { get; set; } = "IN_PROGRESS"; // IN_PROGRESS, COMPLETED, CLAIMED
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        
        public virtual Quest Quest { get; set; }
    }

    // Lịch sử hoạt động & Giao dịch
    public class PlayerActivityLog
    {
        [Key] public string LogId { get; set; }
        public string UserId { get; set; }
        public string ActionType { get; set; } // TRANSACTION, CRAFT, HUNT, LOGIN
        public string Details { get; set; } // Nội dung chi tiết
        public string RelatedId { get; set; } // ItemID liên quan
        public int ValueChange { get; set; } // Số tiền/số lượng thay đổi
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
    
    // Hòm thư (Mailbox)
    public class PlayerMail
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? AttachedItemId { get; set; }
        public int AttachedAmount { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsClaimed { get; set; } = false;
        public DateTime SentDate { get; set; } = DateTime.Now;
        
        [ForeignKey("AttachedItemId")] public virtual Item AttachedItem { get; set; }
    }
    
    // Thành tựu (Achievement)
    public class Achievement
    {
        [Key] public string Id { get; set; } // ACH_KILL_100
        public string Name { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public int TargetAmount { get; set; }
        public int RewardGem { get; set; }
    }

    public class PlayerAchievement
    {
        [Key] public int Id { get; set; }
        public string UserId { get; set; }
        public string AchievementId { get; set; }
        public int CurrentProgress { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
}