using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using rodri_movie_mvc.Data;
using rodri_movie_mvc.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// incluir dbcontext
builder.Services.AddDbContext<MovieDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Connection1")));

// incluir identity en tabla existente de usuarios
builder.Services.AddIdentityCore<Usuario>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MovieDbContext>()
    .AddSignInManager();

// agrego manejo de sesion en cookie.
builder.Services.AddAuthentication(options => options.DefaultScheme = IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

// configuracion de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Usuario/Login";
    options.AccessDeniedPath = "/Usuario/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
