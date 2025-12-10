using System;
using System.Collections.Generic;

namespace MinecraftBackend.DTOs
{
    // --- 1. SHOP & INVENTORY ---
    public class ShopItemDto
    {
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public int PriceAmount { get; set; }
        public string PriceCurrency { get; set; } // "RES_GOLD" or "RES_GEM"
        public string Rarity { get; set; }
        public string Type { get; set; } 
        
        // [FIX] Bổ sung trường này để khớp với GameApiController
        public string TargetItemID { get; set; } 
    }

    public class InventoryDto
    {
        public string InventoryId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public int CurrentDurability { get; set; }
        public int MaxDurability { get; set; }
        public int UpgradeLevel { get; set; }
        public bool IsEquipped { get; set; }
    }

    public class BuyRequestDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // --- 2. LEADERBOARD ---
    public class LeaderboardEntryDto
    {
        public string DisplayName { get; set; }
        public int Level { get; set; }
        public string AvatarUrl { get; set; }
        public int Gold { get; set; }
    }

    // --- 3. WIKI ---
    public class WikiEntryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductImage { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
    }

    // --- 4. MAILBOX ---
    public class MailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AttachedItemId { get; set; }
        public string AttachedItemName { get; set; }
        public int AttachedAmount { get; set; }
        public bool IsRead { get; set; }
        public bool IsClaimed { get; set; }
        public string SentDate { get; set; }
    }

    // --- 5. DAILY CHECKIN ---
    public class DailyCheckinResponse
    {
        public string Message { get; set; }
        public int Gold { get; set; }
        public int Streak { get; set; }
    }

    // --- 6. CHAT ---
    public class ChatMessageDto
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
    }

    public class SendChatDto
    {
        public string Msg { get; set; }
    }

    // --- 7. QUESTS ---
    public class QuestProgressDto
    {
        public string QuestId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Current { get; set; }
        public int Target { get; set; }
        public string Status { get; set; } 
        public string RewardName { get; set; }
        public string IconUrl { get; set; }
    }
    
    // --- 8. PROFILE UPDATE ---
    public class UpdateProfileDto
    {
        public string CharacterName { get; set; }
        public string AvatarUrl { get; set; }
    }
    
    // --- 9. GAMEPLAY ACTIONS ---
    public class HuntResponse
    {
        public int GoldEarned { get; set; }
        public int ExpEarned { get; set; }
        public bool LevelUp { get; set; }
        public string LootItemName { get; set; }
    }
}