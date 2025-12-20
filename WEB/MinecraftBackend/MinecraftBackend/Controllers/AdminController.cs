using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Data;
using MinecraftBackend.Models;

namespace MinecraftBackend.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- DASHBOARD ---
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalItems = await _context.ShopItems.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalLogs = await _context.Transactions.CountAsync();
            
            var totalGold = await _context.PlayerProfiles.SumAsync(p => p.Gold);
            ViewBag.TotalRevenue = totalGold.ToString("N0");

            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i)).ToList();
            var transactionCounts = new List<int>();

            foreach (var date in last7Days)
            {
                int count = await _context.Transactions.Where(t => t.CreatedAt.Date == date).CountAsync();
                transactionCounts.Add(count);
            }

            ViewBag.ChartLabels = string.Join(",", last7Days.Select(d => d.ToString("dd/MM")));
            ViewBag.ChartData = string.Join(",", transactionCounts);
            return View();
        }

        // --- ITEMS MANAGEMENT ---
        public async Task<IActionResult> Items(string search = "", int page = 1, string sortOrder = "", string tab = "shop", string typeFilter = "", string rarityFilter = "", string currencyFilter = "", int? minPrice = null, int? maxPrice = null)
        {
            int pageSize = 10;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.TypeSortParm = sortOrder == "Type" ? "type_desc" : "Type";
            ViewBag.RaritySortParm = sortOrder == "Rarity" ? "rarity_desc" : "Rarity";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            
            ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = search; ViewBag.CurrentTab = tab; ViewBag.CurrentType = typeFilter; ViewBag.CurrentRarity = rarityFilter; ViewBag.CurrentCurrency = currencyFilter; ViewBag.CurrentMinPrice = minPrice; ViewBag.CurrentMaxPrice = maxPrice;

            var baseQuery = _context.ShopItems.AsQueryable();
            if (tab == "data") { baseQuery = baseQuery.Where(i => !i.IsShow); ViewBag.Title = "Game Database Assets"; }
            else { baseQuery = baseQuery.Where(i => i.IsShow); ViewBag.Title = "Shop Merchandise"; }

            ViewBag.AvailableTypes = await baseQuery.Select(i => i.ItemType).Distinct().OrderBy(t => t).ToListAsync();
            ViewBag.AvailableRarities = await baseQuery.Select(i => i.Rarity).Distinct().OrderBy(r => r).ToListAsync();

            var query = baseQuery;
            if (!string.IsNullOrEmpty(search)) { string term = search.ToLower(); query = query.Where(i => i.Name.ToLower().Contains(term) || i.ProductID.ToLower().Contains(term)); }
            if (!string.IsNullOrEmpty(typeFilter)) query = query.Where(i => i.ItemType == typeFilter);
            if (!string.IsNullOrEmpty(rarityFilter)) query = query.Where(i => i.Rarity == rarityFilter);
            if (!string.IsNullOrEmpty(currencyFilter)) query = query.Where(i => i.PriceCurrency == currencyFilter);
            if (minPrice.HasValue) query = query.Where(i => i.PriceAmount >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(i => i.PriceAmount <= maxPrice.Value);

            query = sortOrder switch { "name_desc" => query.OrderByDescending(s => s.Name), "Type" => query.OrderBy(s => s.ItemType), "type_desc" => query.OrderByDescending(s => s.ItemType), "Rarity" => query.OrderBy(s => s.Rarity), "rarity_desc" => query.OrderByDescending(s => s.Rarity), "Price" => query.OrderBy(s => s.PriceAmount), "price_desc" => query.OrderByDescending(s => s.PriceAmount), _ => tab == "data" ? query.OrderBy(s => s.ItemType).ThenBy(s => s.Name) : query.OrderBy(s => s.Name) };

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            return View(await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        public IActionResult CreateItem() => View();
        [HttpPost] public async Task<IActionResult> CreateItem(ShopItem item, IFormFile? imageFile) { if (item.PriceAmount < 0) { ModelState.AddModelError("PriceAmount", "Price cannot be negative!"); return View(item); } if (imageFile != null) item.ImageURL = await SaveImage(imageFile, item.ItemType); else item.ImageURL = "/images/others/default.png"; if (string.IsNullOrEmpty(item.TargetItemID)) item.TargetItemID = item.ProductID; ModelState.Remove("ImageURL"); ModelState.Remove("TargetItemID"); if (ModelState.IsValid) { if (await _context.ShopItems.AnyAsync(i => i.ProductID == item.ProductID)) { ModelState.AddModelError("ProductID", "ID này đã tồn tại!"); return View(item); } _context.ShopItems.Add(item); await _context.SaveChangesAsync(); return RedirectToAction("Items", new { tab = item.IsShow ? "shop" : "data" }); } return View(item); }

        public async Task<IActionResult> EditItem(string id) { var item = await _context.ShopItems.FindAsync(id); if (item == null) return NotFound(); return View(item); }
        [HttpPost] public async Task<IActionResult> EditItem(ShopItem item, IFormFile? imageFile) { ModelState.Remove("ImageURL"); ModelState.Remove("TargetItemID"); if (item.PriceAmount < 0) { ModelState.AddModelError("PriceAmount", "Price cannot be negative!"); return View(item); } var existing = await _context.ShopItems.AsNoTracking().FirstOrDefaultAsync(i => i.ProductID == item.ProductID); if (existing == null) return NotFound(); if (imageFile != null) item.ImageURL = await SaveImage(imageFile, item.ItemType); else item.ImageURL = existing.ImageURL; if (string.IsNullOrEmpty(item.TargetItemID)) item.TargetItemID = item.ProductID; if (ModelState.IsValid) { _context.ShopItems.Update(item); await _context.SaveChangesAsync(); return RedirectToAction("Items", new { tab = item.IsShow ? "shop" : "data" }); } return View(item); }

        public async Task<IActionResult> DeleteItem(string id) { var item = await _context.ShopItems.FindAsync(id); string returnTab = "shop"; if (item != null) { returnTab = item.IsShow ? "shop" : "data"; _context.ShopItems.Remove(item); await _context.SaveChangesAsync(); } return RedirectToAction("Items", new { tab = returnTab }); }

        // --- USER MANAGEMENT ---
        public async Task<IActionResult> Users() => View(await _context.PlayerProfiles.Include(p => p.User).ToListAsync());
        [HttpGet] public IActionResult CreateUser() => View();
        [HttpPost] public async Task<IActionResult> CreateUser(string username, string email, string password) { if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) { ModelState.AddModelError("", "Vui lòng nhập đủ thông tin!"); return View(); } if (await _context.Users.AnyAsync(u => u.Email == email || u.Username == username)) { ModelState.AddModelError("", "Email hoặc Username đã tồn tại!"); return View(); } var newUser = new User { Id = Guid.NewGuid().ToString(), Username = username, Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), CreatedAt = DateTime.Now }; _context.Users.Add(newUser); var newProfile = new PlayerProfile { CharacterID = Guid.NewGuid().ToString(), UserId = newUser.Id, DisplayName = username, Level = 1, Gold = 2000, Gem = 50, Health = 100, MaxHealth = 100, Hunger = 100, AvatarUrl = "/images/avatars/steve.png", GameMode = "Survival" }; _context.PlayerProfiles.Add(newProfile); await _context.SaveChangesAsync(); return RedirectToAction("Users"); }
        public async Task<IActionResult> UserDetails(string id) { var profile = await _context.PlayerProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == id); if (profile == null) return NotFound(); ViewBag.Inventory = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync(); ViewBag.Logs = await _context.Transactions.Where(l => l.UserId == id).OrderByDescending(l => l.CreatedAt).Take(20).ToListAsync(); return View(profile); }
        public async Task<IActionResult> EditUser(string id) { var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == id); if (profile == null) return NotFound(); return View(profile); }
        [HttpPost] public async Task<IActionResult> EditUser(PlayerProfile profile, IFormFile? avatarFile) { var existing = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == profile.CharacterID); if (existing == null) return NotFound(); if (avatarFile != null) { string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars"); if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder); string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName; string filePath = Path.Combine(uploadsFolder, uniqueFileName); using (var fileStream = new FileStream(filePath, FileMode.Create)) { await avatarFile.CopyToAsync(fileStream); } existing.AvatarUrl = $"/images/avatars/{uniqueFileName}"; } existing.DisplayName = profile.DisplayName; existing.Level = profile.Level; existing.Gold = profile.Gold; existing.Gem = profile.Gem; existing.GameMode = profile.GameMode; await _context.SaveChangesAsync(); return RedirectToAction("UserDetails", new { id = existing.UserId }); }
        public async Task<IActionResult> ToggleUserStatus(string id) { var user = await _context.Users.FindAsync(id); if (user != null) { user.Status = (user.Status == "Active") ? "Banned" : "Active"; await _context.SaveChangesAsync(); } return RedirectToAction("Users"); }
        [HttpPost] public async Task<IActionResult> ResetUserPassword(string userId, string newPassword) { if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) { TempData["Error"] = "Mật khẩu mới phải dài hơn 6 ký tự!"; return RedirectToAction("Users"); } var user = await _context.Users.FindAsync(userId); if (user != null) { user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword); await _context.SaveChangesAsync(); TempData["Message"] = $"Đã đổi mật khẩu cho user: {user.Username}"; } else TempData["Error"] = "Không tìm thấy người chơi!"; return RedirectToAction("Users"); }
        [HttpPost] public async Task<IActionResult> GiveGift(string userId, string type, int amount, string? itemId) { var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId); if (profile != null) { string logCurrency = "NONE"; int logAmount = 0; if (type == "Gold") { profile.Gold += amount; logCurrency = "RES_GOLD"; logAmount = amount; } else if (type == "Gem") { profile.Gem += amount; logCurrency = "RES_GEM"; logAmount = amount; } else if (type == "Item" && !string.IsNullOrEmpty(itemId)) { _context.Inventories.Add(new GameInventory { InventoryId = Guid.NewGuid().ToString(), UserId = profile.UserId, ItemID = itemId, Quantity = amount, AcquiredDate = DateTime.Now }); } _context.Transactions.Add(new Transaction { UserId = userId, ActionType = "GIFT", Details = $"Admin sent {amount}x {type} {(type == "Item" ? itemId : "")}", CreatedAt = DateTime.Now, CurrencyType = logCurrency, Amount = logAmount, ItemId = (type == "Item" ? itemId : null) }); await _context.SaveChangesAsync(); } return RedirectToAction("UserDetails", new { id = userId }); }

        // --- SYSTEM LOGS ---
        public async Task<IActionResult> Logs(string search = "", string userId = "", string type = "", string startDate = "", string endDate = "", string currency = "", string sortOrder = "", int page = 1) { int pageSize = 15; ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_asc" : ""; ViewBag.UserSortParm = sortOrder == "User" ? "user_desc" : "User"; ViewBag.ActionSortParm = sortOrder == "Action" ? "action_desc" : "Action"; ViewBag.AmountSortParm = sortOrder == "Amount" ? "amount_desc" : "Amount"; ViewBag.CurrentSort = sortOrder; ViewBag.CurrentSearch = search; ViewBag.CurrentUserId = userId; ViewBag.CurrentType = type; ViewBag.CurrentStartDate = startDate; ViewBag.CurrentEndDate = endDate; ViewBag.CurrentCurrency = currency; ViewBag.Types = new List<string> { "LOGIN", "REGISTER", "TRANSACTION", "GIFT", "CRAFT", "BUY", "BUILD", "SELL" }; var query = _context.Transactions.AsQueryable(); if (!string.IsNullOrEmpty(type) && type != "All") query = query.Where(l => l.ActionType == type); if (!string.IsNullOrEmpty(userId)) query = query.Where(l => l.UserId.Contains(userId)); if (!string.IsNullOrEmpty(search)) query = query.Where(l => l.Details.ToLower().Contains(search.ToLower()) || l.ItemId.Contains(search)); if (!string.IsNullOrEmpty(currency)) query = query.Where(l => l.CurrencyType == currency); if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime sDt)) query = query.Where(l => l.CreatedAt >= sDt.Date); if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime eDt)) query = query.Where(l => l.CreatedAt < eDt.Date.AddDays(1)); query = sortOrder switch { "date_asc" => query.OrderBy(l => l.CreatedAt), "User" => query.OrderBy(l => l.UserId), "user_desc" => query.OrderByDescending(l => l.UserId), "Action" => query.OrderBy(l => l.ActionType), "action_desc" => query.OrderByDescending(l => l.ActionType), "Amount" => query.OrderBy(l => l.Amount), "amount_desc" => query.OrderByDescending(l => l.Amount), _ => query.OrderByDescending(l => l.CreatedAt) }; int totalItems = await query.CountAsync(); int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize); page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1)); return View(await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()); }

        // --- SIMULATOR: CẬP NHẬT LOAD BUILDINGS ---
        public async Task<IActionResult> Simulator()
        {
            var players = await _context.PlayerProfiles.Include(p => p.User).ToListAsync();
            
            ViewBag.ShopItems = await _context.ShopItems.Where(i => i.IsShow).OrderBy(i => i.PriceAmount).ToListAsync();
            ViewBag.Recipes = await _context.Recipes.ToListAsync();
            ViewBag.Monsters = await _context.Monsters.ToListAsync();
            
            // FIX: Load danh sách công trình thật từ DB (ItemType = Building)
            ViewBag.Buildings = await _context.ShopItems.Where(i => i.ItemType == "Building").ToListAsync();
            
            return View(players);
        }

        public IActionResult TestApi() => View();
        [HttpPost] public async Task<IActionResult> SeedDataStrict() { SeedData.Initialize(HttpContext.RequestServices); TempData["Message"] = "Data Seeded Successfully!"; return RedirectToAction("Dashboard"); }
        [HttpPost] public async Task<IActionResult> FactoryReset() { _context.Inventories.RemoveRange(_context.Inventories); _context.Transactions.RemoveRange(_context.Transactions); var profiles = await _context.PlayerProfiles.ToListAsync(); foreach (var p in profiles) { p.Gold = 1000; p.Gem = 0; p.Level = 1; p.Exp = 0; p.Health = 100; } await _context.SaveChangesAsync(); TempData["Message"] = "Đã Reset toàn bộ dữ liệu Game!"; return RedirectToAction("Dashboard"); }
        [HttpPost] public IActionResult UpdateMOTD(string msg) { string path = Path.Combine(_env.WebRootPath, "motd.txt"); System.IO.File.WriteAllText(path, msg); TempData["Message"] = $"Đã cập nhật thông báo Server: {msg}"; return RedirectToAction("Dashboard"); }
        private async Task<string> SaveImage(IFormFile image, string type) { string folder = type?.ToLower() switch { "weapon" => "weapons", "armor" => "armor", "consumable" => "resources", "resource" => "resources", "bundle" => "others", _ => "others" }; string uploadsFolder = Path.Combine(_env.WebRootPath, "images", folder); if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder); string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName; string filePath = Path.Combine(uploadsFolder, uniqueFileName); using (var fileStream = new FileStream(filePath, FileMode.Create)) { await image.CopyToAsync(fileStream); } return $"/images/{folder}/{uniqueFileName}"; }
    }
}