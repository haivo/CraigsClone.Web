var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

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
