using Application.Interfaces;
using Application.Servicies;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Vintaloo_WebApp.src.Infrastructure.Repositories;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── MVC ──
    builder.Services.AddControllersWithViews();

    // ── DbContext ──
    builder.Services.AddDbContext<VintalooDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ── Repositories ──
    builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
    builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
    builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
    builder.Services.AddScoped<IImagenArticuloRepository, ImagenArticuloRepository>();

    // ── Services ──
    builder.Services.AddScoped<IArticuloService, ArticuloService>();
    builder.Services.AddScoped<ICategoriaService, CategoriaService>();
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<IFavoritoService, FavoritoService>();
    builder.Services.AddScoped<IReporteService, ReporteService>();
    builder.Services.AddScoped<IImagenArticuloService, ImagenArticuloService>();

    // ── Auth Service ──
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ── JWT ──
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey))
        };

        // Para MVC: leer el token desde la cookie en lugar del header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["jwt_token"];
                return Task.CompletedTask;
            }
        };
    });

    // ── Session ──
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    var app = builder.Build();

    // ── Manejo de errores ──
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();        
    app.UseAuthentication(); 
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    var rutaError = Path.Combine(AppContext.BaseDirectory, "error_inicio.txt");
    File.WriteAllText(rutaError, ex.ToString());

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("=== ERROR AL INICIAR LA APLICACIÓN ===");
    Console.WriteLine(ex.Message);
    if (ex.InnerException != null)
        Console.WriteLine("Detalle: " + ex.InnerException.Message);
    Console.ResetColor();
    Console.WriteLine($"\nError completo guardado en: {rutaError}");
    Console.ReadKey();
}

