using Microsoft.AspNetCore.Identity;
using SODERIA_I.Data;
using Microsoft.EntityFrameworkCore;
using System;
using SODERIA_I.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => 
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // Número máximo de reintentos
                maxRetryDelay: TimeSpan.FromSeconds(30), // Retardo máximo entre reintentos
                errorNumbersToAdd: null  // Puedes especificar códigos de error adicionales a los ya manejados
            );
        }));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // ← Esto es lo que faltaba
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login"; // Redirige al login si no está autenticado
    options.AccessDeniedPath = "/Cuenta/AccessDenied"; // Página de acceso denegado
});


// Registrar el servicio de Azure Blob Storage
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<FacturasDigitales>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var usuariosAutorizados = new List<(string email, string password)>
    {
        ("tomasolocco04@gmail.com", "738546Moto!"),
        ("gustavoolocco@hotmail.com", "Gustavo300765!")
    };

    foreach (var (email, password) in usuariosAutorizados)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            await userManager.CreateAsync(user, password);
        }
    }
}
// Registrar soporte para controladores con vistas
app.UseStaticFiles(); // Para servir archivos estáticos como CSS o JS
app.UseRouting();
app.UseAuthorization();
app.UseStaticFiles(); // Habilita el acceso a archivos en wwwroot

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
