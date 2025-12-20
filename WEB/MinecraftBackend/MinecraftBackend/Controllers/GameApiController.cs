using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Data;
using MinecraftBackend.Models;
using MinecraftBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace MinecraftBackend.Controllers
{
    [Route("api/game")]
    [ApiController]
    [Authorize]
    public class GameApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static List<ChatMessageDto> _globalChat = new List<ChatMessageDto>();
        private static List<MailDto> _mockMails = new List<MailDto>();

        public GameApiController(ApplicationDbContext context)
        {
            _context = context;
            if (_mockMails.Count == 0)
            {
                _mockMails.Add(new MailDto { Id = 1, Title = "Welcome!", Content = "Chào mừng bạn đến với Minecraft Server.", SentDate = DateTime.Now.ToString("yyyy-MM-dd"), IsRead = false, IsClaimed = false, AttachedItemId = "WEP_WOOD_SWORD", AttachedItemName = "Wooden Sword", AttachedAmount = 1 });
            }
        }

        private async Task<PlayerProfile> GetCurrentProfile()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Request.Headers.TryGetValue("X-Character-ID", out var charIdStr))
            {
                return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.CharacterID == charIdStr.ToString());
            }
            return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // --- 1. PROFILE ---
        [HttpGet("profile/me")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return NotFound("Character not found.");
            return Ok(new CharacterDto { CharacterID = profile.CharacterID, CharacterName = profile.DisplayName, Level = profile.Level, Exp = profile.Exp, Gold = profile.Gold, Gem = profile.Gem, AvatarUrl = profile.AvatarUrl ?? "/images/avatars/default.png", GameMode = profile.GameMode, Health = profile.Health, MaxHealth = profile.MaxHealth, Hunger = profile.Hunger });
        }

        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.CharacterName)) profile.DisplayName = dto.CharacterName;
            if (!string.IsNullOrEmpty(dto.AvatarUrl)) profile.AvatarUrl = dto.AvatarUrl;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated" });
        }

        // --- 2. SHOP SYSTEM ---
        [HttpGet("shop")]
        public async Task<IActionResult> GetShop(int page = 1, int pageSize = 10)
        {
            var items = await _context.ShopItems.Where(i => i.IsShow).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(i => new ShopItemDto { ProductID = i.ProductID, Name = i.Name, Description = i.Description, ImageURL = i.ImageURL, PriceAmount = i.PriceAmount, PriceCurrency = i.PriceCurrency, Rarity = i.Rarity, Type = i.ItemType, TargetItemID = i.TargetItemID }).ToListAsync();
            return Ok(items);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyItem([FromBody] BuyRequestDto req)
        {
            if (req.Quantity <= 0) return BadRequest("Invalid quantity.");
            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            var product = await _context.ShopItems.FindAsync(req.ProductId);
            if (product == null || !product.IsShow) return BadRequest("Item unavailable.");

            int totalCost = product.PriceAmount * req.Quantity;
            if (product.PriceCurrency == "RES_GOLD") { if (profile.Gold < totalCost) return BadRequest("Not enough Gold!"); profile.Gold -= totalCost; }
            else { if (profile.Gem < totalCost) return BadRequest("Not enough Gem!"); profile.Gem -= totalCost; }

            await AddToInventory(profile.UserId, product.TargetItemID, req.Quantity);
            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "BUY", Details = $"Bought {req.Quantity}x {product.Name}", ItemId = product.TargetItemID, Amount = -totalCost, CurrencyType = product.PriceCurrency, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Purchase successful", newBalance = (product.PriceCurrency == "RES_GOLD" ? profile.Gold : profile.Gem) });
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellItem([FromBody] BuyRequestDto req)
        {
            if (req.Quantity <= 0) return BadRequest("Invalid quantity.");
            var profile = await GetCurrentProfile();
            var invItem = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == req.ProductId);
            if (invItem == null || invItem.Quantity < req.Quantity) return BadRequest("Not enough items.");

            var shopInfo = await _context.ShopItems.FirstOrDefaultAsync(s => s.TargetItemID == req.ProductId);
            int unitPrice = (shopInfo != null) ? Math.Max(1, shopInfo.PriceAmount / 2) : 10;
            string currency = (shopInfo != null) ? shopInfo.PriceCurrency : "RES_GOLD";
            int totalEarn = unitPrice * req.Quantity;

            if (currency == "RES_GOLD") profile.Gold += totalEarn; else profile.Gem += totalEarn;
            invItem.Quantity -= req.Quantity;
            if (invItem.Quantity <= 0) _context.Inventories.Remove(invItem);

            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SELL", Details = $"Sold {req.Quantity}x {req.ProductId}", ItemId = req.ProductId, Amount = totalEarn, CurrencyType = currency, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sold successfully", earned = totalEarn });
        }

        // --- 3. INVENTORY & USAGE ---
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var profile = await GetCurrentProfile();
            var invItems = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync();
            var result = new List<InventoryDto>();
            foreach (var inv in invItems) { var meta = await _context.ShopItems.FirstOrDefaultAsync(s => s.TargetItemID == inv.ItemID); result.Add(new InventoryDto { InventoryId = inv.InventoryId, ItemId = inv.ItemID, Name = meta?.Name ?? inv.ItemID, ImageUrl = meta?.ImageURL ?? "/images/others/default.png", Quantity = inv.Quantity, Type = meta?.ItemType ?? "Misc", Rarity = meta?.Rarity ?? "Common", IsEquipped = inv.IsEquipped, UpgradeLevel = inv.UpgradeLevel, CurrentDurability = inv.CurrentDurability, MaxDurability = 100 }); }
            return Ok(result);
        }
        [HttpPost("use-item/{itemId}")] public async Task<IActionResult> UseItem(string itemId) { var profile = await GetCurrentProfile(); var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == itemId); if (inv == null || inv.Quantity < 1) return BadRequest("Item not found"); inv.Quantity--; if (inv.Quantity <= 0) _context.Inventories.Remove(inv); profile.Health = Math.Min(profile.Health + 20, profile.MaxHealth); await _context.SaveChangesAsync(); return Ok(new { message = "Used item", hp = profile.Health }); }
        [HttpPost("equip/{itemId}")] public async Task<IActionResult> EquipItem(string itemId) { var profile = await GetCurrentProfile(); var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == itemId); if (inv == null) return BadRequest("Item not found"); inv.IsEquipped = !inv.IsEquipped; await _context.SaveChangesAsync(); return Ok(new { message = inv.IsEquipped ? "Equipped" : "Unequipped" }); }

        // --- 4. CRAFTING SYSTEM ---
        [HttpGet("recipes")] public async Task<IActionResult> GetRecipes() { var recipes = await _context.Recipes.Select(r => new { r.RecipeId, r.ResultItemName, r.ResultItemImage, r.CraftingTime, IngredientsStr = r.Ingredients }).ToListAsync(); return Ok(recipes); }
        [HttpPost("craft/{recipeId}")] public async Task<IActionResult> CraftItem(string recipeId) { var profile = await GetCurrentProfile(); if (profile == null) return Unauthorized(); var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeId); if (recipe == null) return BadRequest(new { message = "Recipe not found!" }); var ingredients = recipe.Ingredients.Split('|'); var requiredItems = new Dictionary<string, int>(); foreach (var part in ingredients) { var split = part.Split(':'); if (split.Length == 2 && int.TryParse(split[1], out int qty)) requiredItems[split[0]] = qty; } var userInv = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync(); foreach (var req in requiredItems) { var itemInInv = userInv.FirstOrDefault(i => i.ItemID == req.Key); if (itemInInv == null || itemInInv.Quantity < req.Value) return BadRequest(new { message = $"Thiếu nguyên liệu: {req.Key}" }); } foreach (var req in requiredItems) { var itemInInv = userInv.FirstOrDefault(i => i.ItemID == req.Key); itemInInv.Quantity -= req.Value; if (itemInInv.Quantity <= 0) _context.Inventories.Remove(itemInInv); } await AddToInventory(profile.UserId, recipe.ResultItemId, 1); _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "CRAFT", Details = $"Crafted {recipe.ResultItemName}", Amount = 0, CurrencyType = "NONE", CreatedAt = DateTime.Now }); await _context.SaveChangesAsync(); return Ok(new { message = $"Chế tạo thành công {recipe.ResultItemName}!" }); }

        // --- 5. GAMEPLAY ---
        [HttpPost("daily-checkin")] public async Task<IActionResult> DailyCheckin() { var p = await GetCurrentProfile(); p.Gold += 500; await _context.SaveChangesAsync(); return Ok(new DailyCheckinResponse { Message = "+500G", Gold = 500, Streak = 1 }); }
        
        // FIX: Trả về Monsters từ DB thay vì cứng
        [AllowAnonymous] 
        [HttpGet("monsters")] 
        public async Task<IActionResult> GetMonsters() 
        { 
             return Ok(await _context.Monsters.ToListAsync()); 
        }
        
        [HttpPost("hunt")] 
        public async Task<IActionResult> HuntMonster() { 
            var p = await GetCurrentProfile(); 
            p.Gold += 10; p.Exp += 5; 
            bool lvUp = false; if (p.Exp >= 100 * p.Level) { p.Level++; p.Exp = 0; lvUp = true; } 
            _context.Transactions.Add(new Transaction { UserId = p.UserId, ActionType = "HUNT", Details = "Hunted Monster", Amount = 10, CurrencyType = "RES_GOLD", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync(); 
            return Ok(new HuntResponse { GoldEarned = 10, ExpEarned = 5, LevelUp = lvUp }); 
        }
        
        [HttpGet("chat")] public IActionResult GetChat() => Ok(_globalChat.TakeLast(50));
        [HttpPost("chat")] public async Task<IActionResult> SendChat([FromBody] SendChatDto dto) { var profile = await GetCurrentProfile(); var msg = new ChatMessageDto { Sender = profile.DisplayName, Content = dto.Msg, Time = DateTime.Now.ToString("HH:mm") }; _globalChat.Add(msg); if (_globalChat.Count > 100) _globalChat.RemoveAt(0); return Ok(msg); }
        [HttpGet("my-quests")] public IActionResult GetQuests() => Ok(new List<QuestProgressDto> { new QuestProgressDto { QuestId = "Q1", Name = "First Blood", Description = "Kill 1 Monster", Current = 1, Target = 1, Status = "COMPLETED", RewardName = "100 Gold" } });
        [HttpPost("quests/claim/{id}")] public async Task<IActionResult> ClaimQuest(string id) { var p = await GetCurrentProfile(); p.Gold += 100; await _context.SaveChangesAsync(); return Ok(new { message = "Claimed" }); }
        [HttpGet("mail")] public IActionResult GetMails() => Ok(_mockMails);
        [HttpPost("mail/claim/{id}")] public async Task<IActionResult> ClaimMail(int id) { var mail = _mockMails.FirstOrDefault(m => m.Id == id); if (mail == null || mail.IsClaimed) return BadRequest("Error"); var profile = await GetCurrentProfile(); if (mail.AttachedItemId == "RES_GOLD") profile.Gold += mail.AttachedAmount; else if (!string.IsNullOrEmpty(mail.AttachedItemId)) await AddToInventory(profile.UserId, mail.AttachedItemId, mail.AttachedAmount); mail.IsClaimed = true; mail.IsRead = true; await _context.SaveChangesAsync(); return Ok(new { message = "Claimed" }); }

        // --- 7. SIMULATOR ENDPOINTS ---
        [AllowAnonymous]
        [HttpPost("sim/buy")]
        public async Task<IActionResult> SimBuy(string charId, string prodId)
        {
            try 
            {
                var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
                if (profile == null) return NotFound("Char not found");
                var prod = await _context.ShopItems.FirstOrDefaultAsync(i => i.ProductID == prodId);
                if (prod == null) return BadRequest("Product ID invalid");
                string currency = prod.PriceCurrency ?? "NONE"; 
                if (currency == "RES_GOLD") profile.Gold -= prod.PriceAmount; else if (currency == "RES_GEM") profile.Gem -= prod.PriceAmount;

                await AddToInventory(profile.UserId, prod.TargetItemID, 1);
                _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_BUY", Details = $"Simulated Buy: {prod.Name}", Amount = -prod.PriceAmount, CurrencyType = currency, ItemId = prod.TargetItemID, CreatedAt = DateTime.Now });
                await _context.SaveChangesAsync();
                return Ok($"Simulated purchase: {prod.Name} added.");
            }
            catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpPost("sim/craft")]
        public async Task<IActionResult> SimCraft(string charId, string recipeId)
        {
            try
            {
                var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
                if (profile == null) return NotFound("Char not found");
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeId);
                if (recipe == null) return BadRequest("Recipe ID invalid");

                await AddToInventory(profile.UserId, recipe.ResultItemId, 1);
                _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_CRAFT", Details = $"Simulated Craft: {recipe.ResultItemName}", CreatedAt = DateTime.Now, CurrencyType = "NONE", Amount = 0 });
                await _context.SaveChangesAsync();
                return Ok($"Simulated Force Craft: {recipe.ResultItemName} added.");
            }
            catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
        }
        
        [AllowAnonymous]
        [HttpPost("sim/build")] 
        public async Task<IActionResult> SimBuild(string charId, string buildTypeId) 
        {
            try
            {
                var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
                if (profile == null) return NotFound("Char not found");

                var buildItem = await _context.ShopItems.FirstOrDefaultAsync(i => i.ProductID == buildTypeId);
                string buildName = buildItem?.Name ?? buildTypeId;

                await AddToInventory(profile.UserId, buildTypeId, 1);
                _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_BUILD", Details = $"Simulated Build: {buildName}", CreatedAt = DateTime.Now, CurrencyType = "NONE", Amount = 0 });
                await _context.SaveChangesAsync();
                return Ok($"Simulated Build: {buildName} added to Inventory.");
            }
            catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
        }

        [AllowAnonymous] [HttpPost("sim/quest")] public async Task<IActionResult> SimQuest(string charId, string questId) { try { var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId); if (profile == null) return NotFound("Char not found"); _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_QUEST", Details = $"Simulated Quest Completed: {questId}", CreatedAt = DateTime.Now, CurrencyType = "NONE", Amount = 0 }); await _context.SaveChangesAsync(); return Ok($"Simulated Quest: {questId} (Log Saved)."); } catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); } }
        [AllowAnonymous] [HttpPost("sim/upgrade")] public async Task<IActionResult> SimUpgrade(string charId, string ruleId) { try { var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId); if (profile == null) return NotFound("Char not found"); _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_UPGRADE", Details = $"Simulated Upgrade: {ruleId}", CreatedAt = DateTime.Now, CurrencyType = "NONE", Amount = 0 }); await _context.SaveChangesAsync(); return Ok($"Simulated Upgrade: {ruleId} (Log Saved)."); } catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); } }

        private async Task AddToInventory(string userId, string itemId, int qty) { var existing = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemID == itemId); if (existing != null) existing.Quantity += qty; else _context.Inventories.Add(new GameInventory { InventoryId = Guid.NewGuid().ToString(), UserId = userId, ItemID = itemId, Quantity = qty, AcquiredDate = DateTime.Now }); }

        // --- PUBLIC DATA ENDPOINTS ---
        [AllowAnonymous]
        [HttpGet("affordable/{charId}")]
        public async Task<IActionResult> CheckAffordable(string charId) {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound();
            var items = await _context.ShopItems.Where(i => (i.PriceCurrency == "RES_GOLD" && i.PriceAmount <= profile.Gold) || (i.PriceCurrency == "RES_GEM" && i.PriceAmount <= profile.Gem)).Select(i => new { i.Name, i.PriceAmount, i.PriceCurrency }).ToListAsync();
            return Ok(items);
        }
        
        [AllowAnonymous]
        [HttpGet("top-selling")] 
        public async Task<IActionResult> GetTopSelling() {
            var topItem = await _context.Transactions
                .Where(t => (t.ActionType == "BUY" || t.ActionType == "SIM_BUY") && t.ItemId != null)
                .GroupBy(t => t.ItemId)
                .Select(g => new { ItemId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            if (topItem != null) {
               var name = await _context.ShopItems.Where(s => s.TargetItemID == topItem.ItemId).Select(s => s.Name).FirstOrDefaultAsync();
               return Ok(new { TopItem = name ?? topItem.ItemId, Sales = topItem.Count });
            }
            return Ok(new { TopItem = "None", Sales = 0 });
        }

        [AllowAnonymous][HttpGet("cheap-diamond")] public async Task<IActionResult> GetCheapDiamond() => Ok(await _context.ShopItems.Where(i => i.Name.Contains("Diamond") && i.PriceAmount < 500).ToListAsync());
        
        // FIX: Đổi từ "Resource" thành "Material" để khớp SeedData
        [AllowAnonymous] 
        [HttpGet("resources")] 
        public async Task<IActionResult> GetResourcesSim() 
        { 
            return Ok(await _context.ShopItems.Where(i => i.ItemType == "Material").ToListAsync()); 
        }
        
        // FIX: Lấy Building từ DB thay vì cứng
        [AllowAnonymous] 
        [HttpGet("buildings")] 
        public async Task<IActionResult> GetBuildingsSim() 
        { 
            return Ok(await _context.ShopItems.Where(i => i.ItemType == "Building").ToListAsync()); 
        }
        
        [AllowAnonymous][HttpGet("quests")] public IActionResult GetQuestsSim() => Ok(new List<object> { new { Name = "Kill Dragon", Reward = "Elytra" } });
        [AllowAnonymous][HttpGet("leaderboard")] public async Task<IActionResult> GetLeaderboard() => Ok(await _context.PlayerProfiles.OrderByDescending(p => p.Level).Take(10).ToListAsync());
        [AllowAnonymous] [HttpGet("wiki")] public async Task<IActionResult> GetWiki() => Ok(await _context.ShopItems.ToListAsync());
        [AllowAnonymous][HttpGet("expensive-weapons")] public async Task<IActionResult> GetExpensiveWeapons() { return Ok(await _context.ShopItems.Where(i => i.ItemType == "Weapon" && i.PriceAmount > 100).ToListAsync()); }
        [HttpGet("transactions/my")] public async Task<IActionResult> GetMyTransactions() { var profile = await GetCurrentProfile(); if (profile == null) return Unauthorized(); var logs = await _context.Transactions.Where(l => l.UserId == profile.UserId).OrderByDescending(l => l.CreatedAt).Take(20).Select(l => new { Id = l.Id, Action = l.Details, Amount = l.Amount, Date = l.CreatedAt.ToString("dd/MM/yyyy HH:mm"), Currency = l.CurrencyType }).ToListAsync(); return Ok(logs); }
    }
}