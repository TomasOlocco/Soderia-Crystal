using Microsoft.AspNetCore.Identity;
using SODERIA_I.Data;
using Microsoft.EntityFrameworkCore;
using System;
using SODERIA_I.Services;


var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar soporte para controladores con vistas
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<FacturasDigitales>();


var app = builder.Build();

app.UseStaticFiles(); // Para servir archivos estáticos como CSS o JS
app.UseRouting();
app.UseAuthorization();
app.UseStaticFiles(); // Habilita el acceso a archivos en wwwroot

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
