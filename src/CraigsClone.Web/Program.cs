using CraigsClone.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Development only: bring the database up to date and seed it, so "docker compose up"
// then "dotnet run" is all a fresh clone needs. Elsewhere, run migrations deliberately.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Attribute routing only. Every URL is declared on its action with [HttpGet("...")].
// The template's conventional "{controller}/{action}" route is gone on purpose:
// it would compete with /{citySlug}/{categorySlug}.
app.MapControllers().WithStaticAssets();

app.Run();

// Top-level statements make Program internal. Tests need WebApplicationFactory<Program>.
public partial class Program { }
