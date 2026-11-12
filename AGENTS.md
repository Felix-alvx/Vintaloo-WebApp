# AGENTS.md

## Build and run

- The solution lives in `Vintaloo.WebApp/`, not the repo root. Run commands from there; root-level `dotnet build` fails.
  ```powershell
  cd Vintaloo.WebApp
  dotnet build Vintaloo.WebApp.sln
  dotnet run --project Vintaloo.WebApp.csproj
  ```
- Launch profiles: HTTPS `https://localhost:7056`, HTTP `http://localhost:5279`, IIS Express `http://localhost:9674`.
- Test project: `Vintaloo.Tests/Vintaloo.Tests.csproj` at the repo root. No linter, formatter, or CI workflows exist. Verification is `dotnet build Vintaloo.WebApp.sln` plus `dotnet test ..\Vintaloo.Tests\Vintaloo.Tests.csproj` from `Vintaloo.WebApp` (baseline: 0 errors; existing CS8604 nullability warnings in repository SQL calls).
- SQL Server with database `vintaloo_db` is required at runtime. The DDL/stored-procedure script is tracked at `docs/database/vintaloo_schema.sql`; it was generated from the live schema and validated against an isolated database. The app builds without a database, but page actions that hit data will fail.

## Architecture and naming traps

- Dependency flow: `Vintaloo.WebApp` -> `Application` + `Infrastructure`; `Infrastructure` -> `Application` + `Domain`; `Application` -> `Domain`.
- Domain entities are under `Domain/Entities/` but use namespace `Domain.Data`, not `Domain.Entities`. Match this when adding entities.
- Application services are under `Application/Servicies/` (misspelled) and use namespace `Application.Servicies`. Do not create a parallel `Services` folder.
- Controllers depend on `Application.Interfaces` service interfaces; services depend on repository interfaces; concrete repositories and `VintalooDbContext` live in `Infrastructure`. Register every new repository/service in `Program.cs` (all registrations are `Scoped`).
- DTOs live in `Application/DTOs`; mapping from entities to DTOs is done manually in services.
- Code uses Spanish identifiers and action names (`obtenerArticulos`, `GuardarArticulos`, `Crear`, `Detalle`, `EditarPerfil`, `TempData["Mensaje"]`). Follow the existing language and naming style.

## Data access

- Database-first EF Core: there are no migrations. Do not add migrations or rely on `EnsureCreated`.
- Writes use stored procedures via `_context.Database.ExecuteSqlRaw("EXEC sp_...")`. Inserts that need the generated ID use `FromSqlRaw` over the keyless `IdResult` entity (see `ArticuloRepository.GuardarArticulos`).
- Reads are mixed: some use LINQ over `DbSet`, others use `FromSqlRaw("EXEC sp_...")`.
- Stored procedures hardcoded in repositories define the expected database contract, including `sp_insertar_articulo`, `sp_cambiar_estado_articulo`, `sp_eliminar_articulo`, `sp_obtener_categoria`, `sp_insertar_categoria`, `sp_actualizar_categoria`, `sp_eliminar_categoria`, `sp_agregar_fav`, `sp_eliminar_favorito`, `sp_obtener_imagenes_articulo`, `sp_insertar_imagen_articulo`, `sp_obtener_reportes`, `sp_crear_reporte`, `sp_insertar_usuario`, `sp_obtener_usuarios_porId`, `sp_actualizar_usuario`, and `sp_eliminar_usuario`.
- Role data is read/written with raw SQL against `roles` and `usuario_roles`; those tables have no entity classes.

## Authentication

- JWT is stored in the HttpOnly cookie `jwt_token`, not an `Authorization: Bearer` header. `Program.cs` configures `JwtBearerEvents.OnMessageReceived` to read the cookie.
- `AuthController` sets/clears the cookie and also uses session (`UsuarioNombre`). Keep `UseSession()` before `UseAuthentication()`.
- There are `[Authorize]` attributes on protected controllers/actions. Controllers also manually call `ObtenerIdUsuarioActual()` (from `ClaimTypes.NameIdentifier`, the `id_usuario`) and redirect to `Auth/Login` when it returns 0. Follow this pattern for per-resource checks.
- JWT settings live under `Jwt`. The signing key must come from environment variables or `appsettings.Development.json` (gitignored); never commit secrets to `appsettings.json`.

## Routing, config, files

- The default MVC route is `{controller=Dashboard}/{action=Index}/{id?}`, not `Home`. `Home` is only the landing page.
- The connection string comes from `ConnectionStrings:DefaultConnection` in environment variables or `appsettings.Development.json` (gitignored); `appsettings.json` leaves it empty. `VintalooDbContext.OnConfiguring` no longer hardcodes it.
- `appsettings.Development.json` is gitignored.
- Uploads use GUID filenames: article images go to `wwwroot/Imagen/` (capital `I` on disk, referenced as `/imagen/...`) and profile photos to `wwwroot/perfiles/`.
- `Program.cs` validates the connection string and JWT configuration at startup, fails fast by rethrowing startup exceptions, adds security headers (including CSP) and rate limiting. `/health` checks database connectivity.

## README caveat

- `README.md` is stale/aspirational in places (for example, it says only the landing page exists and lists controllers as pending), while auth, services, and most controllers are already implemented. Trust the code and config over README status claims.
