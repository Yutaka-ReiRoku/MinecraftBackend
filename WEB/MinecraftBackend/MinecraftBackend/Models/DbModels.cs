using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinecraftBackend.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }

    public class PlayerProfile
    {
        [Key]
        public string CharacterID { get; set; }
        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string GameMode { get; set; }

        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Hunger { get; set; } = 100;

        public int Gold { get; set; } = 0;
        public int Gem { get; set; } = 0;

        public int LoginStreak { get; set; } = 0;
        public DateTime? LastLoginDate { get; set; }
    }
    public class ShopItem
    {
        [Key]
        public string ProductID { get; set; }
        public string TargetItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public int PriceAmount { get; set; }
        public string PriceCurrency { get; set; }
        public string ItemType { get; set; }
        public string Rarity { get; set; }
        public bool IsShow { get; set; } = true;
    }

    public class GameInventory
    {
        [Key]
        public string InventoryId { get; set; }
        
        public string UserId { get; set; }
        public string ItemID { get; set; }
        public int Quantity { get; set; }
        
        public bool IsEquipped { get; set; } = false;
        public int CurrentDurability { get; set; } = 100;
        public int UpgradeLevel { get; set; } = 0;
        public DateTime AcquiredDate { get; set; }
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; }
        public string? ItemId { get; set; }
        public string CurrencyType { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}