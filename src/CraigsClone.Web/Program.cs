using CraigsClone.Web.Data;
using CraigsClone.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Every POST is checked for the anti-forgery token automatically. The <form method="post">
// tag helper adds the hidden token, so the site's own forms just work.
builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IListingService, ListingService>();

var app = builder.Build();

// Development only: bring the database up to date and seed it, so "docker compose up"
// then "dotnet run" is all a fresh clone needs. Elsewhere, run migrations deliberately.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
    await DevSeeder.SeedAsync(db);   // sample ads, dev only
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");   // in Development the developer exception page shows instead
    app.UseHsts();
}

// 404 (and any other empty error response) gets a real page, inside the layout, with the
// original URL still in the address bar and the original status code preserved.
app.UseStatusCodePagesWithReExecute("/error/{0}");

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
