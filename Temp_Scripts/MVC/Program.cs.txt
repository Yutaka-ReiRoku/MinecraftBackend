using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinecraftBackend.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================== 1. SERVICES CONFIGURATION ====================

// A. Kết nối Database (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=minecraft.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// B. Cấu hình JWT Authentication
var tokenKey = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(tokenKey))
{
    tokenKey = "daylachuoi_bi_mat_sieu_dai_khong_ai_doan_duoc_123456";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// C. Cấu hình CORS (Cho phép mọi nguồn kết nối - Quan trọng cho Unity WebGL/Editor)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddControllersWithViews();

// ==================== 2. APP BUILD ====================

var app = builder.Build();

// D. SEED DATA (Tự động nạp dữ liệu mẫu)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Tạo DB tự động
        SeedData.Initialize(services); // Nạp Item mẫu
        Console.WriteLine(">>> Database Initialized & Seeded Successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> ERROR Seeding DB: {ex.Message}");
    }
}

// ==================== 3. MIDDLEWARE PIPELINE ====================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Chỉ bật HSTS khi deploy thật
    // app.UseHsts(); 
}

// [FIX] TẮT HTTPS REDIRECTION ĐỂ TRÁNH LỖI SSL TRONG UNITY EDITOR
// app.UseHttpsRedirection(); 

app.UseStaticFiles(); // Cho phép tải ảnh từ wwwroot

app.UseRouting();

app.UseCors("AllowAll"); // Phải đặt giữa Routing và Auth

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Dashboard}/{id?}");

Console.WriteLine(">>> Server is running at: http://localhost:5000");

app.Run();