# Estrategia de pruebas

## Objetivo
Detectar regresiones en autenticación, autorización, validaciones, administración y acceso a datos sin usar la base de datos de producción.

## Estado actual
- Proyecto de pruebas: `Vintaloo.Tests/Vintaloo.Tests.csproj`, ubicado en la raíz del repositorio y agregado a `Vintaloo.WebApp/Vintaloo.WebApp.sln`.
- Resultado actual: **76 pruebas correctas, 0 errores**.
- Pruebas unitarias: usan mocks y no tocan SQL Server.
- Pruebas de integración HTTP: usan `WebApplicationFactory<Program>` y no requieren base de datos.
- Pruebas de integración SQL: usan una base aislada creada a partir de `vintaloo_db` y se eliminan al terminar.

## Herramientas
- xUnit para casos y teorías.
- Moq para dobles de repositorios.
- `Microsoft.AspNetCore.Mvc.Testing` para endpoints en memoria.
- `Microsoft.Data.SqlClient` y `DBCC CLONEDATABASE` para crear la base de pruebas.
- Coverlet para cobertura.
- BCrypt para verificar hashes.
- JWT con `System.IdentityModel.Tokens.Jwt` 8.23.0 y `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.34.
- EF Core 8.0.34, `Microsoft.Data.SqlClient` 5.2.3, `System.Text.Json` 8.0.6 y `Microsoft.AspNetCore.Mvc.Testing` 8.0.34.
- `dotnet list Vintaloo.WebApp.sln package --vulnerable --include-transitive` sin paquetes vulnerables.

## Organización
- `AuthServiceTests.cs`: login, token JWT, recuperación y restablecimiento de contraseña con mocks.
- `UsuarioServiceTests.cs`: creación, actualización, roles, contraseñas y reglas de administración.
- `ValidationTests.cs`: política de contraseñas y validaciones de DTOs.
- `AuthorizationTests.cs`: presencia de `[Authorize]` y roles requeridos mediante reflexión.
- `IntegrationTests.cs`: respuestas HTTP de login, recuperación, reset y redirección de rutas protegidas.
- `Integration/SqlServerTestFixture.cs`: crea, reinicia y elimina la base de pruebas.
- `Integration/*RepositorySqlTests.cs`: repositorios de usuarios, categorías, artículos, imágenes, favoritos y reportes contra SQL Server.
- `Integration/AuthServiceSqlTests.cs`: login, registro y recuperación usando SQL Server real.
- `Integration/LoginWebSqlTests.cs`: login HTTP con cookie JWT y validación de acceso admin contra SQL Server.
- `Integration/ArchivoWebSqlTests.cs`: subida de imágenes de artículos y fotos de perfil, validación de extensión y tamaño, nombres GUID y persistencia en SQL Server.
- `Integration/AdminWebSqlTests.cs`: flujo E2E de alta, edición y eliminación de usuarios y categorías, y creación/consulta de reportes.
- `Integration/WebTestSupport.cs`: factory HTTP compartida, login con antiforgery y helper de autenticación.

## Base de datos de pruebas
- Nombre por defecto: `vintaloo_test_db`.
- Origen por defecto: `vintaloo_db`.
- Se crea con `DBCC CLONEDATABASE`, se pasa a `READ_WRITE` y se siembran los roles `admin` y `usuario`.
- Cada prueba limpia las tablas de datos y conserva los roles.
- La base se elimina al finalizar la colección de pruebas.
- Las pruebas de archivos usan un `WebRoot` temporal y lo eliminan al terminar; no escriben en `wwwroot`.
- Variables opcionales:
  - `VINTALOO_TEST_SQL_CONNECTION`: cadena de conexión al servidor.
  - `VINTALOO_TEST_SOURCE_DB`: base de origen para el clon.
  - `VINTALOO_TEST_DB`: nombre de la base de pruebas.

## Requisitos de datos
La base usa restricciones `CHECK`; respetar estos valores:
- `usuarios.estado`: `activo`, `bloqueado`.
- `articulos.estado_producto`: `excelente`, `bueno`, `regular`.
- `articulos.estado_publicacion`: `disponible`, `reservado`, `vendido`.
- `reportes.estado`: `pendiente`, `revisado`, `rechazado`.

## Comandos
Desde `Vintaloo.WebApp`:

```powershell
dotnet build Vintaloo.WebApp.sln
dotnet test ..\Vintaloo.Tests\Vintaloo.Tests.csproj
```

Solo pruebas SQL:

```powershell
dotnet test ..\Vintaloo.Tests\Vintaloo.Tests.csproj --filter "FullyQualifiedName~Vintaloo.Tests.Integration"
```

Cobertura:

```powershell
dotnet test ..\Vintaloo.Tests\Vintaloo.Tests.csproj --collect:"XPlat Code Coverage"
```

## Reglas
- No ejecutar pruebas de escritura contra `vintaloo_db`; solo se usa como plantilla de esquema.
- No usar credenciales reales ni enviar correos reales.
- Las pruebas SQL deben usar `vintaloo_test_db` o una base aislada equivalente.
- Las pruebas deben ser deterministas: sin depender de fechas, orden de ejecución ni datos externos.
- Mantener el estilo de nombres en español usado por el proyecto.

## Pendientes
- Pruebas de recuperación de contraseña con un `IEmailService` simulado; se posponen hasta contar con proveedor de correo.
