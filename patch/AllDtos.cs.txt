using System;
using System.Collections.Generic;

namespace MinecraftBackend.DTOs
{
    // --- AUTHENTICATION (Đăng ký/Đăng nhập) ---

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // --- CHARACTER & PROFILE ---

    public class CreateCharacterDto
    {
        public string Name { get; set; }
        public string GameMode { get; set; } // Survival, Creative, Hardcore
    }

    public class CharacterDto
    {
        public string CharacterID { get; set; }
        public string CharacterName { get; set; }
        public string AvatarUrl { get; set; }
        public string GameMode { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
        public int Gem { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Hunger { get; set; }
        public int MaxHunger { get; set; }
        public int Exp { get; set; }
    }

    // --- SHOP & ITEMS ---

    public class ShopItemDto
    {
        public string ProductID { get; set; } // ID dùng để mua
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public int PriceAmount { get; set; }
        public string PriceCurrency { get; set; } // RES_GOLD hoặc RES_GEM
        public bool IsBundle { get; set; }
        public int SoldCount { get; set; } // Để hiển thị Best Seller
        public bool IsNew { get; set; } // Để hiển thị Badge NEW
        
        // Stats hiển thị tooltip
        public int Attack { get; set; }
        public int Defense { get; set; }
    }

    public class BuyRequestDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string PaymentCurrencyId { get; set; } // Optional: Nếu món hàng cho phép chọn loại tiền
    }

    // --- INVENTORY ---

    public class InventoryDto
    {
        public string InventoryId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; } // Weapon, Armor, Resource...
        public string Rarity { get; set; }
        public int CurrentDurability { get; set; }
        public int MaxDurability { get; set; }
        public int UpgradeLevel { get; set; }
        
        // Stats
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Heal { get; set; }
        public int HungerRestore { get; set; }
    }

    // --- CRAFTING & RECIPES ---

    public class RecipeDto
    {
        public string RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CraftingTime { get; set; } // Giây
        
        // Kết quả
        public string ResultItemId { get; set; }
        public string ResultItemName { get; set; }
        public string ResultItemImage { get; set; }
        public int ResultCount { get; set; }

        // Nguyên liệu (Simplified List để hiển thị UI)
        public List<MaterialDto> Materials { get; set; }
    }

    public class MaterialDto
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int RequiredQty { get; set; }
        public int UserHasQty { get; set; } // Số lượng user đang có (để hiện màu đỏ/xanh)
    }

    // --- GAMEPLAY ACTIONS ---

    public class HuntResponse
    {
        public string Message { get; set; }
        public int GoldEarned { get; set; }
        public int ExpEarned { get; set; }
        public int CurrentHealth { get; set; }
        public bool LevelUp { get; set; }
        public int NewLevel { get; set; }
        public bool IsDead { get; set; }
        
        // Loot Drop
        public string LootItemName { get; set; }
        public string LootItemImage { get; set; }
        public int LootQuantity { get; set; }
        
        // Trạng thái trang bị (để client update nếu bị hỏng)
        public string EquippedWeaponId { get; set; } 
    }

    public class MineResponse
    {
        public string Message { get; set; }
        public int HungerConsumed { get; set; }
        public int CurrentHunger { get; set; }
        public string ResourceName { get; set; }
        public int Quantity { get; set; }
    }

    // --- SYSTEM & LOGS ---

    public class TransactionLogDto
    {
        public string Time { get; set; }
        public string Type { get; set; } // Buy, Sell, Loot
        public string Message { get; set; }
        public int ValueChange { get; set; }
        public string Currency { get; set; }
    }
}