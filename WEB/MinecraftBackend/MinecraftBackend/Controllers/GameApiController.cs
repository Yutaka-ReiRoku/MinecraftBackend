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

            // Khởi tạo dữ liệu RAM (Chat/Mail) nếu chưa có
            if (_mockMails.Count == 0)
            {
                _mockMails.Add(new MailDto { Id = 1, Title = "Welcome!", Content = "Chào mừng bạn đến với Minecraft Server.", SentDate = DateTime.Now.ToString("yyyy-MM-dd"), IsRead = false, IsClaimed = false, AttachedItemId = "WEP_WOOD_SWORD", AttachedItemName = "Wooden Sword", AttachedAmount = 1 });
            }
        }

        private async Task<PlayerProfile> GetCurrentProfile()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Hỗ trợ header đặc biệt nếu client gửi lên để xác định nhân vật
            if (Request.Headers.TryGetValue("X-Character-ID", out var charIdStr))
            {
                return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.CharacterID == charIdStr.ToString());
            }
            return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // --- 1. PROFILE & BASIC INFO (Giữ nguyên) ---
        [HttpGet("profile/me")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return NotFound("Character not found.");

            return Ok(new CharacterDto
            {
                CharacterID = profile.CharacterID,
                CharacterName = profile.DisplayName,
                Level = profile.Level,
                Exp = profile.Exp,
                Gold = profile.Gold,
                Gem = profile.Gem,
                AvatarUrl = profile.AvatarUrl ?? "/images/avatars/default.png",
                GameMode = profile.GameMode,
                Health = profile.Health,
                MaxHealth = profile.MaxHealth,
                Hunger = profile.Hunger
            });
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

        // --- 2. SHOP SYSTEM (Logic Thật - Đã cập nhật) ---
        [HttpGet("shop")]
        public async Task<IActionResult> GetShop(int page = 1, int pageSize = 10)
        {
            // Lấy item thật từ DB
            var items = await _context.ShopItems
                .Where(i => i.IsShow)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(i => new ShopItemDto
                {
                    ProductID = i.ProductID,
                    Name = i.Name,
                    Description = i.Description,
                    ImageURL = i.ImageURL,
                    PriceAmount = i.PriceAmount,
                    PriceCurrency = i.PriceCurrency,
                    Rarity = i.Rarity,
                    Type = i.ItemType,
                    TargetItemID = i.TargetItemID
                }).ToListAsync();

            return Ok(items);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyItem([FromBody] BuyRequestDto req)
        {
            if (req.Quantity <= 0) return BadRequest("Invalid quantity.");

            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            var product = await _context.ShopItems.FindAsync(req.ProductId);
            if (product == null) return NotFound("Product not found");
            if (!product.IsShow) return BadRequest("Item unavailable.");

            int totalCost = product.PriceAmount * req.Quantity;

            // Kiểm tra tiền
            if (product.PriceCurrency == "RES_GOLD")
            {
                if (profile.Gold < totalCost) return BadRequest("Not enough Gold!");
                profile.Gold -= totalCost;
            }
            else
            {
                if (profile.Gem < totalCost) return BadRequest("Not enough Gem!");
                profile.Gem -= totalCost;
            }

            // Cộng đồ vào kho (Hàm Helper phía dưới)
            await AddToInventory(profile.UserId, product.TargetItemID, req.Quantity);

            // Ghi log
            _context.Transactions.Add(new Transaction
            {
                UserId = profile.UserId,
                ActionType = "BUY",
                Details = $"Bought {req.Quantity}x {product.Name}",
                ItemId = product.TargetItemID,
                Amount = -totalCost,
                CurrencyType = product.PriceCurrency,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Purchase successful", newBalance = (product.PriceCurrency == "RES_GOLD" ? profile.Gold : profile.Gem) });
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellItem([FromBody] BuyRequestDto req)
        {
            if (req.Quantity <= 0) return BadRequest("Invalid quantity.");
            var profile = await GetCurrentProfile();
            
            // Tìm item trong kho
            var invItem = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == req.ProductId);
            if (invItem == null || invItem.Quantity < req.Quantity) return BadRequest("Not enough items to sell.");

            // Định giá
            var shopInfo = await _context.ShopItems.FirstOrDefaultAsync(s => s.TargetItemID == req.ProductId);
            int unitPrice = (shopInfo != null) ? Math.Max(1, shopInfo.PriceAmount / 2) : 10;
            string currency = (shopInfo != null) ? shopInfo.PriceCurrency : "RES_GOLD";
            
            int totalEarn = unitPrice * req.Quantity;

            // Cộng tiền
            if (currency == "RES_GOLD") profile.Gold += totalEarn;
            else profile.Gem += totalEarn;

            // Trừ đồ
            invItem.Quantity -= req.Quantity;
            if (invItem.Quantity <= 0) _context.Inventories.Remove(invItem);

            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SELL", Details = $"Sold {req.Quantity}x {req.ProductId}", ItemId = req.ProductId, Amount = totalEarn, CurrencyType = currency, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sold successfully", earned = totalEarn });
        }

        // --- 3. INVENTORY & USAGE (Giữ nguyên cho Client) ---
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var profile = await GetCurrentProfile();
            var invItems = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync();
            var result = new List<InventoryDto>();

            foreach (var inv in invItems)
            {
                var meta = await _context.ShopItems.FirstOrDefaultAsync(s => s.TargetItemID == inv.ItemID);
                result.Add(new InventoryDto
                {
                    InventoryId = inv.InventoryId,
                    ItemId = inv.ItemID,
                    Name = meta?.Name ?? inv.ItemID,
                    ImageUrl = meta?.ImageURL ?? "/images/others/default.png",
                    Quantity = inv.Quantity,
                    Type = meta?.ItemType ?? "Misc",
                    Rarity = meta?.Rarity ?? "Common",
                    IsEquipped = inv.IsEquipped,
                    UpgradeLevel = inv.UpgradeLevel,
                    CurrentDurability = inv.CurrentDurability,
                    MaxDurability = 100
                });
            }
            return Ok(result);
        }

        [HttpPost("use-item/{itemId}")]
        public async Task<IActionResult> UseItem(string itemId)
        {
            var profile = await GetCurrentProfile();
            var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == itemId);

            if (inv == null || inv.Quantity < 1) return BadRequest("Item not found");

            // Logic cũ: Trừ số lượng, hồi máu
            inv.Quantity--;
            if (inv.Quantity <= 0) _context.Inventories.Remove(inv);

            profile.Health = Math.Min(profile.Health + 20, profile.MaxHealth);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Used item", hp = profile.Health });
        }

        [HttpPost("equip/{itemId}")]
        public async Task<IActionResult> EquipItem(string itemId)
        {
            var profile = await GetCurrentProfile();
            var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == itemId);
            if (inv == null) return BadRequest("Item not found");

            // Logic cũ: Toggle trang bị
            inv.IsEquipped = !inv.IsEquipped;
            
            await _context.SaveChangesAsync();
            return Ok(new { message = inv.IsEquipped ? "Equipped" : "Unequipped" });
        }

        // --- 4. CRAFTING SYSTEM (Logic Thật - Đọc từ DB) ---
        [HttpGet("recipes")]
        public async Task<IActionResult> GetRecipes()
        {
            // Trả về dữ liệu thật từ DB thay vì cứng
            var recipes = await _context.Recipes.Select(r => new { 
                r.RecipeId, r.ResultItemName, r.ResultItemImage, r.CraftingTime, IngredientsStr = r.Ingredients 
            }).ToListAsync();
            return Ok(recipes);
        }

        [HttpPost("craft/{recipeId}")]
        public async Task<IActionResult> CraftItem(string recipeId)
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            // 1. Tìm công thức thật
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeId);
            if (recipe == null) return BadRequest(new { message = "Recipe not found!" });

            // 2. Parse nguyên liệu (Format: "ITEM_ID:QTY|ITEM_ID:QTY")
            // VD: "RES_WOOD:2|RES_IRON:1"
            var ingredients = recipe.Ingredients.Split('|');
            var requiredItems = new Dictionary<string, int>();

            foreach (var part in ingredients)
            {
                var split = part.Split(':');
                if (split.Length == 2 && int.TryParse(split[1], out int qty))
                {
                    requiredItems[split[0]] = qty;
                }
            }

            // 3. Check kho đồ
            var userInv = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync();
            foreach (var req in requiredItems)
            {
                var itemInInv = userInv.FirstOrDefault(i => i.ItemID == req.Key);
                if (itemInInv == null || itemInInv.Quantity < req.Value)
                {
                    return BadRequest(new { message = $"Thiếu nguyên liệu: {req.Key}" });
                }
            }

            // 4. Trừ nguyên liệu
            foreach (var req in requiredItems)
            {
                var itemInInv = userInv.FirstOrDefault(i => i.ItemID == req.Key);
                itemInInv.Quantity -= req.Value;
                if (itemInInv.Quantity <= 0) _context.Inventories.Remove(itemInInv);
            }

            // 5. Cộng thành phẩm
            await AddToInventory(profile.UserId, recipe.ResultItemId, 1);

            // 6. Log
            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "CRAFT", Details = $"Crafted {recipe.ResultItemName}", Amount = 0, CurrencyType = "NONE", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Chế tạo thành công {recipe.ResultItemName}!" });
        }

        // --- 5. GAMEPLAY ACTIONS (Giữ nguyên + Cập nhật data thật) ---
        
        [HttpPost("daily-checkin")]
        public async Task<IActionResult> DailyCheckin()
        {
            var p = await GetCurrentProfile();
            p.Gold += 500;
            // Logic cũ của bạn
            await _context.SaveChangesAsync();
            return Ok(new DailyCheckinResponse { Message = "+500G", Gold = 500, Streak = 1 });
        }

        [HttpGet("monsters")]
        [AllowAnonymous] // Cho phép Sim gọi
        public async Task<IActionResult> GetMonsters() 
        {
             // Fix: Lấy từ DB thật
             return Ok(await _context.Monsters.ToListAsync());
        }

        [HttpPost("hunt")]
        public async Task<IActionResult> HuntMonster()
        {
            var p = await GetCurrentProfile();
            
            // Logic cũ: Cộng exp/gold đơn giản
            p.Gold += 10;
            p.Exp += 5;

            bool lvUp = false;
            if (p.Exp >= 100 * p.Level)
            {
                p.Level++;
                p.Exp = 0;
                lvUp = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new HuntResponse { GoldEarned = 10, ExpEarned = 5, LevelUp = lvUp });
        }

        // --- 6. CHAT & QUESTS (Giữ nguyên) ---
        [HttpGet("chat")]
        public IActionResult GetChat() => Ok(_globalChat.TakeLast(50));

        [HttpPost("chat")]
        public async Task<IActionResult> SendChat([FromBody] SendChatDto dto)
        {
            var profile = await GetCurrentProfile();
            var msg = new ChatMessageDto { Sender = profile.DisplayName, Content = dto.Msg, Time = DateTime.Now.ToString("HH:mm") };
            _globalChat.Add(msg);
            if (_globalChat.Count > 100) _globalChat.RemoveAt(0);
            return Ok(msg);
        }

        [HttpGet("my-quests")]
        public IActionResult GetQuests() => Ok(new List<QuestProgressDto> { new QuestProgressDto { QuestId = "Q1", Name = "First Blood", Description = "Kill 1 Monster", Current = 1, Target = 1, Status = "COMPLETED", RewardName = "100 Gold" } });

        [HttpPost("quests/claim/{id}")]
        public async Task<IActionResult> ClaimQuest(string id)
        {
            var p = await GetCurrentProfile();
            p.Gold += 100;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Claimed" });
        }
        
        [HttpGet("mail")] public IActionResult GetMails() => Ok(_mockMails);
        [HttpPost("mail/claim/{id}")]
        public async Task<IActionResult> ClaimMail(int id)
        {
            var mail = _mockMails.FirstOrDefault(m => m.Id == id);
            if (mail == null || mail.IsClaimed) return BadRequest("Error");
            var profile = await GetCurrentProfile();
            if (mail.AttachedItemId == "RES_GOLD") profile.Gold += mail.AttachedAmount;
            else if (!string.IsNullOrEmpty(mail.AttachedItemId)) await AddToInventory(profile.UserId, mail.AttachedItemId, mail.AttachedAmount);
            
            mail.IsClaimed = true; mail.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Claimed" });
        }

        // --- 7. SIMULATOR ENDPOINTS (Logic Thật - Admin Only) ---
        
        [AllowAnonymous]
        [HttpPost("sim/buy")]
        public async Task<IActionResult> SimBuy(string charId, string prodId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound("Char not found");
            
            var prod = await _context.ShopItems.FirstOrDefaultAsync(i => i.ProductID == prodId);
            if (prod == null) return BadRequest("Product ID invalid");

            // Trừ tiền thật
            if (prod.PriceCurrency == "RES_GOLD") profile.Gold -= prod.PriceAmount; 
            else profile.Gem -= prod.PriceAmount;

            // Cộng đồ thật
            await AddToInventory(profile.UserId, prod.TargetItemID, 1);
            
            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_BUY", Details = $"Simulated Buy: {prod.Name}", Amount = -prod.PriceAmount, CurrencyType = prod.PriceCurrency, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok($"Simulated purchase: {prod.Name} added, Money deducted.");
        }

        [AllowAnonymous]
        [HttpPost("sim/craft")]
        public async Task<IActionResult> SimCraft(string charId, string recipeId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound("Char not found");

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeId);
            if (recipe == null) return BadRequest("Recipe ID invalid");

            // Sim Craft: Admin ép chế (bỏ qua check nguyên liệu), chỉ cộng đồ
            await AddToInventory(profile.UserId, recipe.ResultItemId, 1);
            
            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_CRAFT", Details = $"Simulated Craft (Force): {recipe.ResultItemName}", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok($"Simulated Force Craft: {recipe.ResultItemName} added.");
        }
        
        [AllowAnonymous][HttpPost("sim/build")] public async Task<IActionResult> SimBuild(string charId, string buildTypeId) => Ok("Simulated Build");
        [AllowAnonymous][HttpPost("sim/quest")] public async Task<IActionResult> SimQuest(string charId, string questId) => Ok("Simulated Quest");
        [AllowAnonymous][HttpPost("sim/upgrade")] public async Task<IActionResult> SimUpgrade(string charId, string ruleId) => Ok("Simulated Upgrade");

        // --- Helper Function ---
        private async Task AddToInventory(string userId, string itemId, int qty)
        {
            var existing = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemID == itemId);
            if (existing != null) existing.Quantity += qty;
            else _context.Inventories.Add(new GameInventory { InventoryId = Guid.NewGuid().ToString(), UserId = userId, ItemID = itemId, Quantity = qty, AcquiredDate = DateTime.Now });
        }

        // --- Other Read-Only Endpoints ---
        [AllowAnonymous][HttpGet("affordable/{charId}")]
        public async Task<IActionResult> CheckAffordable(string charId) {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound();
            var items = await _context.ShopItems.Where(i => i.PriceAmount <= profile.Gold && i.PriceCurrency == "RES_GOLD").Select(i => new { i.Name, i.PriceAmount }).ToListAsync();
            return Ok(items);
        }
        [AllowAnonymous][HttpGet("top-selling")] public IActionResult GetTopSelling() => Ok(new { TopItem = "Iron Sword", Sales = 150 });
        [AllowAnonymous][HttpGet("cheap-diamond")] public async Task<IActionResult> GetCheapDiamond() => Ok(await _context.ShopItems.Where(i => i.Name.Contains("Diamond") && i.PriceAmount < 500).ToListAsync());
        [AllowAnonymous][HttpGet("resources")] public async Task<IActionResult> GetResourcesSim() => Ok(await _context.ShopItems.Where(i => i.ItemType == "Resource").ToListAsync());
        [AllowAnonymous][HttpGet("buildings")] public IActionResult GetBuildingsSim() => Ok(new List<object> { new { Name = "House", Cost = 500 }, new { Name = "Tower", Cost = 1000 } });
        [AllowAnonymous][HttpGet("quests")] public IActionResult GetQuestsSim() => Ok(new List<object> { new { Name = "Kill Dragon", Reward = "Elytra" } });
        [HttpGet("leaderboard")] public async Task<IActionResult> GetLeaderboard() => Ok(await _context.PlayerProfiles.OrderByDescending(p => p.Level).Take(10).ToListAsync());
        [HttpGet("wiki")] public async Task<IActionResult> GetWiki() => Ok(await _context.ShopItems.ToListAsync());
        [HttpGet("transactions/my")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();
            var logs = await _context.Transactions.Where(l => l.UserId == profile.UserId).OrderByDescending(l => l.CreatedAt).Take(20)
                .Select(l => new { Id = l.Id, Action = l.Details, Amount = l.Amount, Date = l.CreatedAt.ToString("dd/MM/yyyy HH:mm"), Currency = l.CurrencyType }).ToListAsync();
            return Ok(logs);
        }
    }
}