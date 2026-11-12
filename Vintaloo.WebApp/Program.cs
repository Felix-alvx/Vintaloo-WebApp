using Application.Interfaces;
using Application.Servicies;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Vintaloo_WebApp.src.Infrastructure.Repositories;
using Vintaloo.WebApp.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── MVC ──
    builder.Services.AddControllersWithViews();

    // ── DbContext ──
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("La cadena de conexión 'ConnectionStrings:DefaultConnection' no está configurada.");

    builder.Services.AddDbContext<VintalooDbContext>(options =>
        options.UseSqlServer(connectionString));

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
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();

    // ── Health checks ──
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // ── JWT ──
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("La clave JWT 'Jwt:Key' no está configurada.");
    if (jwtKey.Length < 32)
        throw new InvalidOperationException("La clave JWT 'Jwt:Key' debe tener al menos 32 caracteres.");

    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    if (string.IsNullOrWhiteSpace(jwtIssuer))
        throw new InvalidOperationException("El emisor JWT 'Jwt:Issuer' no está configurado.");

    var jwtAudience = builder.Configuration["Jwt:Audience"];
    if (string.IsNullOrWhiteSpace(jwtAudience))
        throw new InvalidOperationException("La audiencia JWT 'Jwt:Audience' no está configurada.");

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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
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
            },
            OnChallenge = context =>
            {
                context.HandleResponse();

                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                var returnUrl = Uri.EscapeDataString(
                    context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Auth/Login?returnUrl={returnUrl}");
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

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        await next();
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();        
    app.UseAuthentication(); 
    app.UseAuthorization();

    app.MapHealthChecks("/health");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("=== ERROR AL INICIAR LA APLICACIÓN ===");
    Console.Error.WriteLine(ex);
    throw;
}

public partial class Program { }

