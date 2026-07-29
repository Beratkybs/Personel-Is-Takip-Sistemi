using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHttpContextAccessor();
// 🎯 GLOBAL FİLTRE ENTEGRASYONU
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionKontrolFiltresi>();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

});

builder.Services.AddScoped<IKullaniciService, KullaniciService>();
builder.Services.AddScoped<IIsTakipService, IsTakipService>();
builder.Services.AddScoped<IProjeService, ProjeService>();
builder.Services.AddScoped<IOrganizasyonService, OrganizasyonService>();
builder.Services.AddScoped<IKategoriService, KategoriService>();
builder.Services.AddScoped<IDurumService, DurumService>();
builder.Services.AddScoped<IIsDetayService, IsDetayService>();
builder.Services.AddScoped<IPlanlamaService, PlanlamaService>();

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



app.UseCookiePolicy();
app.UseSession();
app.UseAuthorization();





app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();

// Temiz bir yapı için Program.cs dosyasının en altına ekleyeceğimiz filtre sınıfı
public class SessionKontrolFiltresi : Microsoft.AspNetCore.Mvc.Filters.IActionFilter
{
    public void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        var routeData = context.RouteData.Values;
        string currentController = routeData["controller"]?.ToString() ?? "";

        // ⚠️ Giriş/Çıkış ekranlarında sonsuz döngü olmaması için kontrolü atla
        if (currentController.Equals("Account", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Session kontrolü
        var session = context.HttpContext.Session;
        if (session.GetInt32("UserId") == null)
        {
            // Giriş yapılmamışsa kullanıcıyı doğrudan Login sayfasına yönlendir
            context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "Account", null);
        }
    }

    public void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
    {
        // Interface gereği implemente edilmelidir, içi boş kalabilir.
    }
}
