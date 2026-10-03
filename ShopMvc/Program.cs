using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    /
    var p = options.ModelBindingMessageProvider;
    p.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị");
    p.SetMissingBindRequiredValueAccessor(f => $"Thiếu giá trị cho {f}");
    p.SetAttemptedValueIsInvalidAccessor((v, f) => $"Giá trị '{v}' không hợp lệ cho {f}");
    p.SetValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ");
    p.SetNonPropertyAttemptedValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ");
    p.SetUnknownValueIsInvalidAccessor(f => $"Giá trị không hợp lệ cho {f}");
    p.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị không hợp lệ");
    p.SetValueMustBeANumberAccessor(f => $"{f} phải là một số hợp lệ");
    p.SetNonPropertyValueMustBeANumberAccessor(() => "Giá trị phải là một số hợp lệ");
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();


var culture = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
