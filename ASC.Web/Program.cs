using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using ASC.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

// Configure Identity (User & Role management)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register RoleManager
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Configure AppSettings
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Register necessary services
builder.Services.AddScoped<IIdentitySeed, IdentitySeed>();
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

// Configure MVC & Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Middleware Pipeline (Request processing)
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
app.UseAuthorization();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize database and seed default data
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Perform Migration before seeding data
    await dbContext.Database.MigrateAsync();

    var storageSeed = serviceProvider.GetRequiredService<IIdentitySeed>();
    await storageSeed.Seed(
        serviceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

await app.RunAsync();
