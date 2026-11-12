using Application.Interfaces;
using Application.Servicies;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Vintaloo_WebApp.src.Infrastructure.Repositories;
using Vintaloo.WebApp.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── MVC ──
    builder.Services.AddControllersWithViews();

    // ── DbContext ──
    builder.Services.AddDbContext<VintalooDbContext>((sp, options) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión 'ConnectionStrings:DefaultConnection' no está configurada.");

        options.UseSqlServer(connectionString);
    });

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
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"]!;
        var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
        var jwtAudience = builder.Configuration["Jwt:Audience"]!;

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
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

    // ── Rate limiting ──
    var loginPermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:LoginPermitLimit")
        ?? (builder.Environment.IsDevelopment() ? 100 : 5);
    var recoveryPermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:RecoveryPermitLimit")
        ?? (builder.Environment.IsDevelopment() ? 50 : 3);

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("login", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = loginPermitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        options.AddPolicy("recuperacion", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = recoveryPermitLimit,
                    Window = TimeSpan.FromMinutes(5),
                    QueueLimit = 0
                }));
    });

    var app = builder.Build();

    // ── Validación de configuración final ──
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("La cadena de conexión 'ConnectionStrings:DefaultConnection' no está configurada.");

    var jwtKey = app.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("La clave JWT 'Jwt:Key' no está configurada.");
    if (jwtKey.Length < 32)
        throw new InvalidOperationException("La clave JWT 'Jwt:Key' debe tener al menos 32 caracteres.");

    var jwtIssuer = app.Configuration["Jwt:Issuer"];
    if (string.IsNullOrWhiteSpace(jwtIssuer))
        throw new InvalidOperationException("El emisor JWT 'Jwt:Issuer' no está configurado.");

    var jwtAudience = app.Configuration["Jwt:Audience"];
    if (string.IsNullOrWhiteSpace(jwtAudience))
        throw new InvalidOperationException("La audiencia JWT 'Jwt:Audience' no está configurada.");

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
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data: https:; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "script-src 'self' 'unsafe-inline'; " +
            "connect-src 'self'; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none';";
        await next();
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
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

