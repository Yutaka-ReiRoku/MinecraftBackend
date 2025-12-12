using System;
using System.Collections.Generic;

namespace MinecraftBackend.DTOs
{
    public class ShopItemDto
    {
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public int PriceAmount { get; set; }
        public string PriceCurrency { get; set; }
        public string Rarity { get; set; }
        public string Type { get; set; } 
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
    public class LeaderboardEntryDto
    {
        public string DisplayName { get; set; }
        public int Level { get; set; }
        public string AvatarUrl { get; set; }
        public int Gold { get; set; }
    }

    public class WikiEntryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductImage { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
    }

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
    public class DailyCheckinResponse
    {
        public string Message { get; set; }
        public int Gold { get; set; }
        public int Streak { get; set; }
    }

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

    public class UpdateProfileDto
    {
        public string CharacterName { get; set; }
        public string AvatarUrl { get; set; }
    }
    
    public class HuntResponse
    {
        public int GoldEarned { get; set; }
        public int ExpEarned { get; set; }
        public bool LevelUp { get; set; }
        public string LootItemName { get; set; }
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

    public class RecipeDto
    {
        public string RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CraftingTime { get; set; } 
        public string ResultItemId { get; set; }
        public string ResultItemName { get; set; }
        public string ResultItemImage { get; set; }
        public int ResultCount { get; set; }
    }
}