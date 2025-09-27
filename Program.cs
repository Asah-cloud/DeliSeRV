using DeliStarter.Components;
using DeliStarter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Force Kestrel to listen on a specific URL
builder.WebHost.UseUrls("http://localhost:5229");

// === Database ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=delistarter.db"));

// === Identity ===
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    // Default: Admins bypass all restrictions
    options.AddPolicy("AdminOverride", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.Identity?.IsAuthenticated == true));
});

// === Razor + Controllers ===
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllersWithViews();   // ✅ keep if you plan APIs
builder.Services.AddHttpContextAccessor();    // ✅ needed for antiforgery in Razor pages

var app = builder.Build();

// === Middleware ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// === Endpoints ===
// Map Blazor components FIRST
app.MapBlazorHub();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Controllers LAST
app.MapControllers();
// === Seed roles + admin user ===
await RoleSeeder.SeedAsync(app.Services);

// Seed Vendors + Adverts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await VendorAdvertSeeder.SeedVendorsAndAdvertsAsync(context, userManager);
}

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}



// === Debug Info (always prints your fixed URL) ===
Console.WriteLine("=== App is running ===");
Console.WriteLine("Listening on: http://localhost:5229");

app.Run();
