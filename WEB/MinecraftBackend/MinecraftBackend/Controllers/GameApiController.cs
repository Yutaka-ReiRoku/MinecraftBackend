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
        private static Dictionary<string, DateTime> _lastCheckin = new Dictionary<string, DateTime>();

        public GameApiController(ApplicationDbContext context)
        {
            _context = context;

            // Khởi tạo thư mẫu nếu chưa có
            if (_mockMails.Count == 0)
            {
                _mockMails.Add(new MailDto { Id = 1, Title = "Welcome!", Content = "Chào mừng bạn đến với Minecraft Server.", SentDate = DateTime.Now.ToString("yyyy-MM-dd"), IsRead = false, IsClaimed = false, AttachedItemId = "WEP_IRON_SWORD", AttachedItemName = "Iron Sword", AttachedAmount = 1 });
                _mockMails.Add(new MailDto { Id = 2, Title = "Beta Gift", Content = "Tặng bạn ít vàng tiêu vặt.", SentDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), IsRead = false, IsClaimed = false, AttachedItemId = "RES_GOLD", AttachedItemName = "Gold", AttachedAmount = 500 });
            }
        }

        private async Task<PlayerProfile> GetCurrentProfile()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu Client gửi kèm ID nhân vật cụ thể thì lấy theo ID đó
            if (Request.Headers.TryGetValue("X-Character-ID", out var charIdStr))
            {
                return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.CharacterID == charIdStr.ToString());
            }
            // Mặc định lấy profile đầu tiên tìm thấy
            return await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

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

        [HttpGet("shop")]
        public async Task<IActionResult> GetShop(int page = 1, int pageSize = 10)
        {
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

            if (!product.IsShow) return BadRequest("This item is no longer for sale.");

            int totalCost = product.PriceAmount * req.Quantity;

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

            // Cộng đồ vào kho
            var existingItem = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == product.TargetItemID);
            if (existingItem != null)
            {
                existingItem.Quantity += req.Quantity;
            }
            else
            {
                _context.Inventories.Add(new GameInventory
                {
                    InventoryId = Guid.NewGuid().ToString(),
                    UserId = profile.UserId,
                    ItemID = product.TargetItemID,
                    Quantity = req.Quantity,
                    AcquiredDate = DateTime.Now
                });
            }

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
            if (profile == null) return Unauthorized();

            // Tìm item trong kho
            var inventoryItem = await _context.Inventories.FirstOrDefaultAsync(i => i.UserId == profile.UserId && i.ItemID == req.ProductId);
            if (inventoryItem == null || inventoryItem.Quantity < req.Quantity)
            {
                return BadRequest("Not enough item quantity to sell.");
            }

            // Định giá bán (bằng 50% giá mua gốc)
            var shopInfo = await _context.ShopItems.FirstOrDefaultAsync(s => s.TargetItemID == req.ProductId);

            int unitPrice = 10;
            string currency = "RES_GOLD";

            if (shopInfo != null)
            {
                unitPrice = Math.Max(1, shopInfo.PriceAmount / 2);
                currency = shopInfo.PriceCurrency;
            }

            int totalEarn = unitPrice * req.Quantity;

            // Cộng tiền
            if (currency == "RES_GOLD") profile.Gold += totalEarn;
            else profile.Gem += totalEarn;

            // Trừ đồ
            inventoryItem.Quantity -= req.Quantity;
            if (inventoryItem.Quantity <= 0)
            {
                _context.Inventories.Remove(inventoryItem);
            }

            // Ghi log
            _context.Transactions.Add(new Transaction
            {
                UserId = profile.UserId,
                ActionType = "SELL",
                Details = $"Sold {req.Quantity}x {req.ProductId}",
                ItemId = req.ProductId,
                Amount = totalEarn,
                CurrencyType = currency,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sold successfully", earned = totalEarn });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            var invItems = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync();
            var result = new List<InventoryDto>();

            foreach (var inv in invItems)
            {
                // Lấy thông tin hiển thị từ ShopItems
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

            // Trừ số lượng
            inv.Quantity--;
            if (inv.Quantity <= 0) _context.Inventories.Remove(inv);

            // Tác dụng: Hồi 20 máu (Ví dụ đơn giản)
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

            // Logic trang bị đơn giản (Toggle)
            if (inv.IsEquipped)
            {
                inv.IsEquipped = false;
            }
            else
            {
                // Có thể thêm logic: Bỏ trang bị món cũ cùng loại trước khi đeo món mới
                inv.IsEquipped = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = inv.IsEquipped ? "Equipped" : "Unequipped" });
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var list = await _context.PlayerProfiles
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Gold)
                .Take(10)
                .Select(p => new LeaderboardEntryDto
                {
                    DisplayName = p.DisplayName,
                    Level = p.Level,
                    AvatarUrl = p.AvatarUrl,
                    Gold = p.Gold
                }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("wiki")]
        public async Task<IActionResult> GetWiki() => Ok(await _context.ShopItems.Select(i => new WikiEntryDto { Id = i.ProductID, Name = i.Name, ProductImage = i.ImageURL, Type = i.ItemType, IsUnlocked = true }).ToListAsync());

        [HttpGet("mail")]
        public IActionResult GetMails() => Ok(_mockMails);

        [HttpPost("mail/claim/{id}")]
        public async Task<IActionResult> ClaimMail(int id)
        {
            var mail = _mockMails.FirstOrDefault(m => m.Id == id);
            if (mail == null || mail.IsClaimed) return BadRequest("Error or already claimed");

            var profile = await GetCurrentProfile();

            // Xử lý nhận quà
            if (mail.AttachedItemId == "RES_GOLD") profile.Gold += mail.AttachedAmount;
            // (Thêm logic nhận Item nếu cần)

            mail.IsClaimed = true;
            mail.IsRead = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Claimed" });
        }

        [HttpGet("chat")]
        public IActionResult GetChat() => Ok(_globalChat.TakeLast(50));

        [HttpPost("chat")]
        public async Task<IActionResult> SendChat([FromBody] SendChatDto dto)
        {
            var profile = await GetCurrentProfile();
            var msg = new ChatMessageDto { Sender = profile.DisplayName, Content = dto.Msg, Time = DateTime.Now.ToString("HH:mm") };
            _globalChat.Add(msg);

            // Giới hạn lịch sử chat
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

        [HttpPost("daily-checkin")]
        public async Task<IActionResult> DailyCheckin()
        {
            var p = await GetCurrentProfile();
            p.Gold += 500;
            // Logic kiểm tra ngày có thể thêm ở đây
            await _context.SaveChangesAsync();
            return Ok(new DailyCheckinResponse { Message = "+500G", Gold = 500, Streak = 1 });
        }

        [HttpPost("hunt")]
        public async Task<IActionResult> HuntMonster()
        {
            var p = await GetCurrentProfile();
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

        // --- HỆ THỐNG CRAFTING (CHẾ TẠO) ---

        [HttpGet("recipes")]
        public IActionResult GetRecipes() => Ok(new List<object> { new { RecipeId = "R1", ResultItemName = "Iron Sword", ResultItemImage = "/images/weapons/iron_sword.png", CraftingTime = 3 } });

        [HttpPost("craft/{recipeId}")]
        public async Task<IActionResult> CraftItem(string recipeId)
        {
            // 1. Xác định vật phẩm cần chế tạo (Hardcode logic cho R1 = Kiếm Sắt)
            string targetItemId = "";

            if (recipeId == "R1") targetItemId = "WEP_IRON_SWORD";

            if (string.IsNullOrEmpty(targetItemId))
                return BadRequest(new { message = "Không tìm thấy công thức này!" });

            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            // 2. Thêm vật phẩm vào kho đồ (Database)
            // Lưu ý: Code này chưa trừ nguyên liệu để bạn test cho dễ
            _context.Inventories.Add(new GameInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                UserId = profile.UserId,
                ItemID = targetItemId,
                Quantity = 1,
                AcquiredDate = DateTime.Now,
                IsEquipped = false,
                CurrentDurability = 100,
                UpgradeLevel = 0
            });

            // 3. Ghi lại lịch sử giao dịch để kiểm tra
            _context.Transactions.Add(new Transaction
            {
                UserId = profile.UserId,
                ActionType = "CRAFT",
                Details = $"Chế tạo thành công: {targetItemId}",
                Amount = 0,
                CurrencyType = "NONE",
                CreatedAt = DateTime.Now
            });

            // 4. Lưu thay đổi xuống Database
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chế tạo thành công!" });
        }

        [HttpGet("transactions/my")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var profile = await GetCurrentProfile();
            if (profile == null) return Unauthorized();

            var logs = await _context.Transactions
                .Where(l => l.UserId == profile.UserId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(20)
                .Select(l => new
                {
                    Id = l.Id,
                    Action = l.Details,
                    Amount = l.Amount,
                    Date = l.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    Currency = l.CurrencyType
                })
                .ToListAsync();
            return Ok(logs);
        }

        // --- SIMULATOR & TESTING ENDPOINTS ---
        // Các API này dùng cho trang Admin Simulator, không cần Auth

        [AllowAnonymous]
        [HttpPost("sim/buy")]
        public async Task<IActionResult> SimBuy(string charId, string prodId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound("Character not found");

            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_BUY", Details = $"Simulated Buy: {prodId}", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok($"Simulated purchase of {prodId} for {profile.DisplayName}");
        }

        [AllowAnonymous]
        [HttpPost("sim/craft")]
        public async Task<IActionResult> SimCraft(string charId, string recipeId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound("Character not found");
            _context.Transactions.Add(new Transaction { UserId = profile.UserId, ActionType = "SIM_CRAFT", Details = $"Simulated Craft: {recipeId}", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();
            return Ok($"Simulated crafting for {profile.DisplayName}");
        }
        [AllowAnonymous][HttpPost("sim/build")] public async Task<IActionResult> SimBuild(string charId, string buildTypeId) => Ok("Simulated Build");
        [AllowAnonymous][HttpPost("sim/quest")] public async Task<IActionResult> SimQuest(string charId, string questId) => Ok("Simulated Quest");
        [AllowAnonymous][HttpPost("sim/upgrade")] public async Task<IActionResult> SimUpgrade(string charId, string ruleId) => Ok("Simulated Upgrade");

        [AllowAnonymous]
        [HttpGet("affordable/{charId}")]
        public async Task<IActionResult> CheckAffordable(string charId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == charId);
            if (profile == null) return NotFound();
            var items = await _context.ShopItems.Where(i => i.PriceAmount <= profile.Gold).Select(i => i.Name).ToListAsync();
            return Ok(items);
        }

        [AllowAnonymous][HttpGet("top-selling")] public IActionResult GetTopSelling() => Ok(new { TopItem = "Diamond Sword", Sales = 150 });
        [AllowAnonymous][HttpGet("expensive-weapons")] public async Task<IActionResult> GetExpensiveWeapons() => Ok(await _context.ShopItems.Where(i => i.ItemType == "Weapon" && i.PriceAmount > 100).ToListAsync());
        [AllowAnonymous][HttpGet("cheap-diamond")] public async Task<IActionResult> GetCheapDiamond() => Ok(await _context.ShopItems.Where(i => i.Name.Contains("Diamond") && i.PriceAmount < 500).ToListAsync());
        [AllowAnonymous][HttpGet("resources")] public async Task<IActionResult> GetResourcesSim() => Ok(await _context.ShopItems.Where(i => i.ItemType == "Resource").ToListAsync());
        [AllowAnonymous][HttpGet("monsters")] public IActionResult GetMonstersSim() => Ok(new List<object> { new { Name = "Zombie", HP = 100 }, new { Name = "Skeleton", HP = 80 } });
        [AllowAnonymous][HttpGet("buildings")] public IActionResult GetBuildingsSim() => Ok(new List<object> { new { Name = "House", Cost = 500 }, new { Name = "Tower", Cost = 1000 } });
        [AllowAnonymous][HttpGet("quests")] public IActionResult GetQuestsSim() => Ok(new List<object> { new { Name = "Kill Dragon", Reward = "Elytra" } });
    }
}