using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using ASC.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 🟢 Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("⚠ Connection string 'DefaultConnection' not found.");

// 🟢 Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

// 🟢 Cấu hình Identity (Hỗ trợ quản lý user & role)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 🟢 Đăng ký RoleManager
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// 🟢 Cấu hình AppSettings
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("AppSettings"));

// 🟢 Đăng ký các dịch vụ cần thiết
builder.Services.AddScoped<IIdentitySeed, IdentitySeed>();
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

// 🟢 Cấu hình Session
builder.Services.AddDistributedMemoryCache();  // ✅ Thêm hỗ trợ bộ nhớ cache
builder.Services.AddSession();  // ✅ Thêm Session vào ứng dụng
// 🟢 Cấu hình Session với thời gian timeout và cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Thời gian session hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;  // Chỉ cho phép truy cập qua HTTP (bảo mật hơn)
    options.Cookie.IsEssential = true;  // Bắt buộc lưu session ngay cả khi tắt cookie không cần thiết
});

// 🟢 Cấu hình MVC & Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();  // ✅ Quan trọng: Thêm Razor Pages để tránh lỗi
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// 🔵 Middleware Pipeline (Xử lý request)
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();  // ✅ Kích hoạt Session
app.UseAuthorization();

// 🔵 Cấu hình route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🔵 Map Razor Pages (Quan trọng để tránh lỗi)
app.MapRazorPages();

// 🔵 Thực hiện seed data (tạo tài khoản mặc định)
await using var scope = app.Services.CreateAsyncScope();
var serviceProvider = scope.ServiceProvider;
var storageSeed = serviceProvider.GetRequiredService<IIdentitySeed>();

await storageSeed.Seed(
    serviceProvider.GetRequiredService<UserManager<IdentityUser>>(),
    serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
    serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()
);
app.UseSession();  // ✅ Đảm bảo được gọi trước Authorization
app.UseAuthorization();

app.Run();
