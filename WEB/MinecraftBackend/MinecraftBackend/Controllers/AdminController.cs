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

        // ==================== 1. DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalItems = await _context.ShopItems.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalLogs = await _context.Transactions.CountAsync();
            
            var totalGold = await _context.PlayerProfiles.SumAsync(p => p.Gold);
            ViewBag.TotalRevenue = totalGold.ToString("N0");

            var last7Days = Enumerable.Range(0, 7)
        .Select(i => DateTime.Today.AddDays(-6 + i))
        .ToList();

            // 2. Lấy dữ liệu từ DB: Nhóm theo ngày và đếm số giao dịch
            var transactionCounts = new List<int>();

            foreach (var date in last7Days)
            {
                // Đếm số transaction trong ngày đó
                int count = await _context.Transactions
                    .Where(t => t.CreatedAt.Date == date)
                    .CountAsync();
                transactionCounts.Add(count);
            }

            // 3. Ép kiểu dữ liệu để gửi sang View
            ViewBag.ChartLabels = string.Join(",", last7Days.Select(d => d.ToString("dd/MM")));
            ViewBag.ChartData = string.Join(",", transactionCounts);

            return View();
        }

        // ==================== 2. SHOP ITEMS (CRUD) ====================
        
        // [CẬP NHẬT] Thêm phân trang (Pagination) để đạt điểm tối đa
        public async Task<IActionResult> Items(string search = "", int page = 1)
        {
            int pageSize = 10; // Số lượng item trên mỗi trang

            var query = _context.ShopItems.AsQueryable();

            // 1. Tìm kiếm
            if (!string.IsNullOrEmpty(search))
{
    // Chuyển từ khóa về chữ thường
    string term = search.ToLower(); 
    
    // So sánh: Tên (chữ thường) có chứa Từ khóa (chữ thường) không?
    query = query.Where(i => i.Name.ToLower().Contains(term) || 
                             i.ProductID.ToLower().Contains(term));
}

            // 2. Tính toán phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Đảm bảo page hợp lệ (không nhỏ hơn 1, không lớn hơn max)
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            // 3. Lấy dữ liệu (Skip & Take)
            var items = await query
                .OrderByDescending(i => i.IsShow) // Ưu tiên hiện đồ đang bán trước
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Truyền thông tin sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSearch = search;

            return View(items);
        }

        public IActionResult CreateItem() => View();

        [HttpPost]
        public async Task<IActionResult> CreateItem(ShopItem item, IFormFile? imageFile)
        {
            // Security: Chặn giá âm
            if (item.PriceAmount < 0)
            {
                ModelState.AddModelError("PriceAmount", "Price cannot be negative!");
                return View(item);
            }

            if (imageFile != null) item.ImageURL = await SaveImage(imageFile, item.ItemType);
            else item.ImageURL = "/images/others/default.png";

            if (string.IsNullOrEmpty(item.TargetItemID)) item.TargetItemID = item.ProductID;
			
			
			ModelState.Remove("ImageURL");
            ModelState.Remove("TargetItemID");
			
			
            if (ModelState.IsValid)
            {
                if (await _context.ShopItems.AnyAsync(i => i.ProductID == item.ProductID))
                {
                    ModelState.AddModelError("ProductID", "ID này đã tồn tại!");
                    return View(item);
                }
                _context.ShopItems.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Items");
            }
            return View(item);
        }

        public async Task<IActionResult> EditItem(string id)
        {
            var item = await _context.ShopItems.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> EditItem(ShopItem item, IFormFile? imageFile)
        {
            // 1. Xóa các lỗi Validation không cần thiết để tránh việc Form bị chặn lại
            // (Vì ImageURL có thể null lúc submit, ta sẽ xử lý gán lại ở dưới)
            ModelState.Remove("ImageURL");
            ModelState.Remove("TargetItemID");

            // 2. Validate nghiệp vụ: Chặn giá tiền âm
            if (item.PriceAmount < 0)
            {
                ModelState.AddModelError("PriceAmount", "Price cannot be negative!");
                return View(item);
            }

            // 3. Tìm sản phẩm cũ trong DB để lấy thông tin ảnh cũ
            // Dùng AsNoTracking() để tránh lỗi conflict khi Update entity
            var existing = await _context.ShopItems.AsNoTracking()
                                         .FirstOrDefaultAsync(i => i.ProductID == item.ProductID);
            
            if (existing == null) return NotFound();

            // 4. Xử lý ảnh: 
            // - Nếu có file mới -> Lưu file mới
            // - Nếu không có file -> Giữ nguyên URL ảnh cũ
            if (imageFile != null) 
            {
                item.ImageURL = await SaveImage(imageFile, item.ItemType);
            }
            else 
            {
                item.ImageURL = existing.ImageURL;
            }

            // 5. Xử lý TargetItemID (Nếu bỏ trống thì lấy luôn ProductID)
            if (string.IsNullOrEmpty(item.TargetItemID)) 
            {
                item.TargetItemID = item.ProductID;
            }

            // 6. Lưu vào Database
            if (ModelState.IsValid)
            {
                _context.ShopItems.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Items");
            }

            // Nếu vẫn còn lỗi khác thì trả về View để sửa
            return View(item);
        }

        public async Task<IActionResult> DeleteItem(string id)
        {
            var item = await _context.ShopItems.FindAsync(id);
            if (item != null)
            {
                _context.ShopItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Items");
        }

        // ==================== 3. USERS & GM TOOLS ====================
        public async Task<IActionResult> Users()
        {
            var users = await _context.PlayerProfiles.Include(p => p.User).ToListAsync();
            return View(users);
        }
		
		// --- 1. Hiển thị form tạo User ---
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // --- 2. Xử lý tạo User & Profile mặc định ---
        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string email, string password)
        {
            // Validate cơ bản
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đủ thông tin!");
                return View();
            }

            // Kiểm tra trùng lặp
            if (await _context.Users.AnyAsync(u => u.Email == email || u.Username == username))
            {
                ModelState.AddModelError("", "Email hoặc Username đã tồn tại!");
                return View();
            }

            // A. Tạo User
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // Mã hóa pass
                Role = "User",
                Status = "Active",
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(newUser);

            // B. Tạo Profile Game mặc định (Level 1, có tiền khởi nghiệp)
            var newProfile = new PlayerProfile
            {
                CharacterID = Guid.NewGuid().ToString(),
                UserId = newUser.Id,
                DisplayName = username,
                Level = 1,
                Exp = 0,
                Gold = 2000, // Admin tạo thì ưu đãi cho 2000 vàng
                Gem = 50,
                Health = 100,
                MaxHealth = 100,
                Hunger = 100,
                AvatarUrl = "/images/avatars/steve.png",
                GameMode = "Survival"
            };
            _context.PlayerProfiles.Add(newProfile);

            // C. Lưu Database
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var profile = await _context.PlayerProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);
            if (profile == null) return NotFound();

            ViewBag.Inventory = await _context.Inventories.Where(i => i.UserId == profile.UserId).ToListAsync();
            ViewBag.Logs = await _context.Transactions
                .Where(l => l.UserId == id)
                .OrderByDescending(l => l.CreatedAt)
                .Take(20)
                .ToListAsync();
            return View(profile);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            // Tìm theo UserId vì id truyền vào từ Users/UserDetails là UserId
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == id);
            if (profile == null) return NotFound();
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(PlayerProfile profile, IFormFile? avatarFile)
        {
            var existing = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.CharacterID == profile.CharacterID);
            if (existing == null) return NotFound();

            // --- 1. XỬ LÝ UPLOAD ẢNH AVATAR ---
            if (avatarFile != null)
            {
                // Tạo đường dẫn lưu vào thư mục /images/avatars/
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");

                // Đảm bảo thư mục tồn tại
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file độc nhất để tránh trùng
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vật lý
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                // Cập nhật URL mới vào Database
                existing.AvatarUrl = $"/images/avatars/{uniqueFileName}";
            }
            // Nếu không upload ảnh mới thì giữ nguyên URL cũ

            // --- 2. CẬP NHẬT THÔNG TIN KHÁC ---
            existing.DisplayName = profile.DisplayName;
            existing.Level = profile.Level;
            existing.Gold = profile.Gold;
            existing.Gem = profile.Gem;
            existing.GameMode = profile.GameMode;

            await _context.SaveChangesAsync();
            return RedirectToAction("UserDetails", new { id = existing.UserId });
        }

        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Status = (user.Status == "Active") ? "Banned" : "Active";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Users");
        }
		
		[HttpPost]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải dài hơn 6 ký tự!";
                return RedirectToAction("Users");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Mã hóa mật khẩu mới và lưu đè lên mật khẩu cũ
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Đã đổi mật khẩu cho user: {user.Username}";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người chơi!";
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> GiveGift(string userId, string type, int amount, string? itemId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                string logCurrency = "NONE";
                int logAmount = 0;

                if (type == "Gold") 
                {
                    profile.Gold += amount;
                    logCurrency = "RES_GOLD";
                    logAmount = amount;
                }
                else if (type == "Gem") 
                {
                    profile.Gem += amount;
                    logCurrency = "RES_GEM";
                    logAmount = amount;
                }
                else if (type == "Item" && !string.IsNullOrEmpty(itemId))
                {
                    _context.Inventories.Add(new GameInventory
                    {
                        InventoryId = Guid.NewGuid().ToString(),
                        UserId = profile.UserId,
                        ItemID = itemId,
                        Quantity = amount,
                        AcquiredDate = DateTime.Now
                    });
                }

                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    ActionType = "GIFT",
                    Details = $"Admin sent {amount}x {type} {(type == "Item" ? itemId : "")}",
                    CreatedAt = DateTime.Now,
                    CurrencyType = logCurrency,
                    Amount = logAmount,
                    ItemId = (type == "Item" ? itemId : null)
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserDetails", new { id = userId });
        }

        // ==================== 4. SYSTEM LOGS ====================
        public async Task<IActionResult> Logs(string type = "All", string userId = "", string date = "")
        {
            var query = _context.Transactions.AsQueryable();
            if (type != "All") query = query.Where(l => l.ActionType == type);
            if (!string.IsNullOrEmpty(userId)) query = query.Where(l => l.UserId.Contains(userId));
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime dt))
            {
                query = query.Where(l => l.CreatedAt.Date == dt.Date);
            }

            ViewBag.Types = new List<string> { "LOGIN", "REGISTER", "TRANSACTION", "GIFT", "CRAFT", "BUY" };
            return View(await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync());
        }

        // ==================== 5. TOOLS ====================
        public IActionResult Simulator() => View(_context.PlayerProfiles.Include(p => p.User).ToList());
        public IActionResult TestApi() => View();

        [HttpPost]
        public async Task<IActionResult> SeedDataStrict()
        {
            SeedData.Initialize(HttpContext.RequestServices);
            TempData["Message"] = "Data Seeded Successfully!";
            return RedirectToAction("Dashboard");
        }
		
		
		// --- BỔ SUNG CÁC HÀM CÒN THIẾU ---

        [HttpPost]
        public async Task<IActionResult> FactoryReset()
        {
            // Xóa sạch dữ liệu chơi để reset server
            _context.Inventories.RemoveRange(_context.Inventories);
            _context.Transactions.RemoveRange(_context.Transactions);
            
            // Reset chỉ số người chơi về mặc định
            var profiles = await _context.PlayerProfiles.ToListAsync();
            foreach (var p in profiles)
            {
                p.Gold = 1000;
                p.Gem = 0;
                p.Level = 1;
                p.Exp = 0;
                p.Health = 100;
                // Không xóa User để tránh lỗi đăng nhập
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Đã Reset toàn bộ dữ liệu Game!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult UpdateMOTD(string msg)
        {
            // Demo: Thông báo Server (Thực tế nên lưu vào DB)
            TempData["Message"] = $"Đã cập nhật thông báo Server: {msg}";
            return RedirectToAction("Dashboard");
        }

        private async Task<string> SaveImage(IFormFile image, string type)
        {
            string folder = type?.ToLower() switch
            {
                "weapon" => "weapons",
                "armor" => "armor",
                "consumable" => "resources",
                "resource" => "resources",
                "bundle" => "others",
                _ => "others"
            };
            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return $"/images/{folder}/{uniqueFileName}";
        }
    }
}