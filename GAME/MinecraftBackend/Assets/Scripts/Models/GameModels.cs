using System;
using System.Collections.Generic;
using Newtonsoft.Json; 

// [FIX] Chuyển toàn bộ JsonProperty sang PascalCase (Viết Hoa Chữ Đầu)
// Để khớp 100% với DTO trên Server ASP.NET Core

// --- 1. AUTHENTICATION ---
[Serializable]
public class TokenResponse
{
    [JsonProperty("Token")] public string Token;
    [JsonProperty("UserId")] public string UserId;
    [JsonProperty("Username")] public string Username;
    [JsonProperty("Message")] public string Message;
}

[Serializable]
public class RegisterRequest 
{ 
    [JsonProperty("Username")] public string Username; 
    [JsonProperty("Email")] public string Email; 
    [JsonProperty("Password")] public string Password;
}

[Serializable]
public class LoginRequest 
{ 
    [JsonProperty("Email")] public string Email; 
    [JsonProperty("Password")] public string Password;
}

// --- 2. CHARACTER & PROFILE ---
[Serializable]
public class CharacterDto
{
    [JsonProperty("characterID")] public string CharacterID;
    [JsonProperty("characterName")] public string CharacterName;
    [JsonProperty("level")] public int Level;
    [JsonProperty("exp")] public int Exp;
    [JsonProperty("gold")] public int Gold;
    [JsonProperty("gem")] public int Gem;
    [JsonProperty("avatarUrl")] public string AvatarUrl;
    [JsonProperty("gameMode")] public string GameMode;
    [JsonProperty("health")] public int Health;
    [JsonProperty("maxHealth")] public int MaxHealth;
    [JsonProperty("hunger")] public int Hunger;
}

[Serializable]
public class CreateCharacterDto 
{ 
    [JsonProperty("CharacterName")] public string CharacterName; 
    [JsonProperty("GameMode")] public string GameMode;
}

// --- 3. SHOP & ITEMS ---
[Serializable]
public class ShopItemDto
{
    [JsonProperty("productID")] public string ProductID;
    [JsonProperty("name")] public string Name;
    [JsonProperty("description")] public string Description;
    [JsonProperty("imageURL")] public string ImageURL;
    [JsonProperty("priceAmount")] public int PriceAmount;
    [JsonProperty("priceCurrency")] public string PriceCurrency;
    [JsonProperty("rarity")] public string Rarity;
    [JsonProperty("type")] public string Type;
    [JsonProperty("targetItemID")] public string TargetItemID;
}

// [FIX QUAN TRỌNG] Sửa lại thành PascalCase để Server đọc được Quantity > 0
[Serializable]
public class BuyRequest 
{ 
    [JsonProperty("ProductId")] public string ProductId; 
    [JsonProperty("Quantity")] public int Quantity;
}

// --- 4. INVENTORY ---
[Serializable]
public class InventoryDto
{
    [JsonProperty("inventoryId")] public string InventoryId;
    [JsonProperty("itemId")] public string ItemId;
    [JsonProperty("name")] public string Name;
    [JsonProperty("imageUrl")] public string ImageUrl;
    [JsonProperty("quantity")] public int Quantity;
    [JsonProperty("type")] public string Type;
    [JsonProperty("rarity")] public string Rarity;
    [JsonProperty("currentDurability")] public int CurrentDurability;
    [JsonProperty("maxDurability")] public int MaxDurability;
    [JsonProperty("upgradeLevel")] public int UpgradeLevel;
    [JsonProperty("isEquipped")] public bool IsEquipped;
}

// --- 5. GAMEPLAY ---
[Serializable]
public class MonsterDto 
{ 
    [JsonProperty("id")] public string Id; 
    [JsonProperty("name")] public string Name;
    [JsonProperty("hp")] public int HP; 
    [JsonProperty("maxHp")] public int MaxHp; 
    [JsonProperty("level")] public int Level; 
    [JsonProperty("imageUrl")] public string ImageUrl;
}

[Serializable]
public class HuntResponse 
{ 
    [JsonProperty("goldEarned")] public int GoldEarned; 
    [JsonProperty("expEarned")] public int ExpEarned; 
    [JsonProperty("levelUp")] public bool LevelUp;
    [JsonProperty("lootItemName")] public string LootItemName; 
}

[Serializable]
public class RecipeDto 
{ 
    [JsonProperty("recipeId")] public string RecipeId; 
    [JsonProperty("resultItemName")] public string ResultItemName;
    [JsonProperty("resultItemImage")] public string ResultItemImage; 
    [JsonProperty("craftingTime")] public float CraftingTime; 
}

// --- 6. SOCIAL & SYSTEMS ---
[Serializable]
public class ChatMessageDto 
{ 
    [JsonProperty("sender")] public string Sender;
    [JsonProperty("content")] public string Content; 
    [JsonProperty("time")] public string Time; 
}

[Serializable]
public class MailDto
{
    [JsonProperty("id")] public int Id;
    [JsonProperty("title")] public string Title;
    [JsonProperty("content")] public string Content;
    [JsonProperty("attachedItemId")] public string AttachedItemId;
    [JsonProperty("attachedItemName")] public string AttachedItemName;
    [JsonProperty("attachedAmount")] public int AttachedAmount;
    [JsonProperty("isRead")] public bool IsRead;
    [JsonProperty("isClaimed")] public bool IsClaimed;
    [JsonProperty("sentDate")] public string SentDate;
}

[Serializable]
public class LeaderboardEntryDto 
{ 
    [JsonProperty("displayName")] public string DisplayName; 
    [JsonProperty("level")] public int Level; 
    [JsonProperty("avatarUrl")] public string AvatarUrl;
    [JsonProperty("gold")] public int Gold; 
}

[Serializable]
public class DailyCheckinResponse 
{ 
    [JsonProperty("message")] public string Message; 
    [JsonProperty("gold")] public int Gold;
    [JsonProperty("streak")] public int Streak; 
}

[Serializable]
public class WikiEntryDto 
{ 
    [JsonProperty("id")] public string Id; 
    [JsonProperty("name")] public string Name;
    [JsonProperty("productImage")] public string ProductImage; 
    [JsonProperty("type")] public string Type; 
    [JsonProperty("isUnlocked")] public bool IsUnlocked;
}

[Serializable]
public class QuestProgressDto 
{ 
    [JsonProperty("questId")] public string QuestId; 
    [JsonProperty("name")] public string Name; 
    [JsonProperty("description")] public string Description;
    [JsonProperty("current")] public int Current; 
    [JsonProperty("target")] public int Target; 
    [JsonProperty("status")] public string Status; 
    [JsonProperty("rewardName")] public string RewardName; 
    [JsonProperty("iconUrl")] public string IconUrl;
}

// --- 7. TRANSACTION HISTORY ---
[Serializable]
public class TransactionDto
{
    [JsonProperty("id")] public int Id;
    [JsonProperty("action")] public string Action;
    [JsonProperty("amount")] public int Amount;
    [JsonProperty("currency")] public string Currency;
    [JsonProperty("date")] public string Date;
}