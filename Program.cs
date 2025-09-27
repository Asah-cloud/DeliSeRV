using DeliStarter.Components;
using DeliStarter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Use Render’s PORT if available, else default to 5229 locally
var port = Environment.GetEnvironmentVariable("PORT") ?? "5229";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

// === Apply migrations + seed data ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // ✅ Run migrations first
    db.Database.Migrate();

    // ✅ Seed roles + admin user
    await RoleSeeder.SeedAsync(services);

    // ✅ Seed vendors + adverts
    await VendorAdvertSeeder.SeedVendorsAndAdvertsAsync(db, userManager);
}

// === Debug Info (prints nicer URL) ===
Console.WriteLine("=== App is running ===");
if (app.Environment.IsDevelopment())
    Console.WriteLine($"Listening on: http://localhost:{port}");
else
    Console.WriteLine($"Listening on: http://0.0.0.0:{port}");

app.Run();
