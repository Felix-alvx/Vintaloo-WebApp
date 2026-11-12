# Auditoría inicial del proyecto Vintaloo

**Fecha:** 10 de noviembre de 2026  
**Fase:** 0 — Auditoría completa (sin modificación de código de aplicación)  
**Alcance:** repositorio completo, solución, capas, controladores, vistas, JavaScript, configuración, acceso a datos, seguridad y preparación para producción.  
**Rutas:** todas las rutas de archivo son relativas a la raíz del repositorio `Vintalooo_WebApp/`.

---

## Leyenda de clasificación

- **CRITICAL:** impide operar de forma segura o desplegar; requiere corrección antes de cualquier otra fase.
- **HIGH:** fallo funcional o de seguridad relevante; debe corregirse antes de producción.
- **MEDIUM:** riesgo o deuda que afecta mantenibilidad, rendimiento o robustez.
- **LOW:** mejora recomendable, inconsistencia menor o deuda cosmética.

**Nivel de certeza**

- **Hecho verificado:** confirmado leyendo el código o ejecutando un comando local.
- **Recomendación:** acción propuesta por ingeniería, no una regla existente en el código.
- **Suposición:** requiere confirmación del propietario del proyecto.

> No se ejecutó la aplicación contra la base de datos ni se probaron flujos en navegador. Los hallazgos dependientes de SQL Server se basan en el contrato visible en el código (nombres de stored procedures y entidades) y deben validarse en la Fase 1.

---

## 1. Resumen ejecutivo

Vintaloo es una aplicación ASP.NET Core 8 MVC con arquitectura en capas (Domain, Application, Infrastructure, WebApp), EF Core 8.0.5, SQL Server database-first y autenticación JWT en cookie HttpOnly. Compila correctamente: **0 errores y 14 advertencias** (incluida `NU1902` por `System.IdentityModel.Tokens.Jwt` 7.0.3).

El proyecto tiene una base funcional real: registro, login, catálogo, detalle de artículo, publicación con imágenes, favoritos, perfil y cambio de estado. Sin embargo, **no está listo para producción**. Los riesgos principales son:

1. **No existe autorización efectiva.** No hay `[Authorize]` ni políticas; varias acciones permiten modificar o eliminar datos de cualquier usuario, e incluso sin sesión.
2. **No se puede desplegar de forma reproducible.** El esquema de base de datos y los stored procedures no están en el repositorio (`docs/database/vintaloo_schema.sql` no existe).
3. **La configuración de producción no es posible sin recompilar.** La cadena de conexión está duplicada y codificada en `VintalooDbContext.OnConfiguring`; el secreto JWT está versionado.
4. **La validación de entrada es prácticamente inexistente** en servidor (DTOs sin anotaciones), y la subida de archivos no valida tipo, tamaño ni extensión.
5. **Hay rutas y vistas rotas.** Varios controladores devuelven vistas que no existen y una consulta de usuarios ejecuta por error un stored procedure de inserción.
6. **No hay pruebas automatizadas, CI/CD, observabilidad ni estrategia de almacenamiento persistente de imágenes.**

**Conclusión:** se debe aprobar y ejecutar primero una fase de estabilización crítica (autorización, configuración, esquema y validaciones) antes de añadir funcionalidades o desplegar.

---

## 2. Stack tecnológico real

| Componente | Versión / estado | Evidencia |
|---|---|---|
| .NET | 8.0 (`net8.0`) | `Domain/Domain.csproj`, `Application/Application.csproj`, `Infrastructure/Infrastructure.csproj`, `Vintaloo.WebApp/Vintaloo.WebApp.csproj` |
| ASP.NET Core | MVC con controladores y Razor Views | `Vintaloo.WebApp/Program.cs` |
| EF Core | 8.0.5 (SQL Server, Relational, Design, Tools) | `Infrastructure/Infrastructure.csproj`, `Domain/Domain.csproj` |
| Base de datos | SQL Server, base `vintaloo_db`, enfoque database-first | `Infrastructure/Data/VintalooDbContext.cs`, `Vintaloo.WebApp/appsettings.json` |
| Autenticación | JWT Bearer leído desde cookie `jwt_token` | `Program.cs`, `Controllers/AuthController.cs`, `Application/Servicies/AuthService.cs` |
| Hash de contraseñas | BCrypt.Net-Next 4.1.0 | `Application/Application.csproj`, `AuthService.cs` |
| JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0 y `System.IdentityModel.Tokens.Jwt` 7.0.3 | `Vintaloo.WebApp.csproj`, `Application.csproj` |
| Frontend | Razor, CSS propio, Bootstrap 5, jQuery, jQuery Validation | `Views/`, `wwwroot/lib/` |
| Pruebas | **No existen** | no hay proyecto de tests en la solución |
| Contenedores | **No existen** | no hay `Dockerfile` ni `docker-compose` |
| CI/CD | **No existe** | `.github/workflows/` está vacío |
| Migraciones EF | **No existen** (database-first, stored procedures) | no hay carpeta `Migrations` |
| Documentación de esquema | **No existe** | `README.md` referencia `docs/database/vintaloo_schema.sql`, ausente |

### Advertencias de compilación verificadas

Comando ejecutado: `dotnet build Vintaloo.WebApp.sln --nologo` desde `Vintaloo.WebApp/`.

- 0 errores.
- 14 advertencias:
  - `NU1902`: vulnerabilidad moderada conocida en `System.IdentityModel.Tokens.Jwt` 7.0.3.
  - `CS1030`: cadena de conexión embebida en `VintalooDbContext.cs`.
  - `CS8604` (12): posibles argumentos nulos en llamadas `ExecuteSqlRaw`/`FromSqlRaw` en repositorios.

---

## 3. Arquitectura real y evaluación

### 3.1 Estructura y dirección de dependencias

```
Vintaloo.WebApp (MVC)
  ├── Application (Interfaces, DTOs, Servicies)
  │      └── Domain (Entidades)
  └── Infrastructure (DbContext, Repositorios)
         ├── Application
         └── Domain
```

Verificado en los `.csproj`. La dirección general es correcta para una arquitectura en capas con inversión de dependencias, pero hay desviaciones:

- **Domain depende de EF Core** (`Domain/Domain.csproj:10-11`). Las entidades usan atributos de persistencia (`[Table]`, `[Column]`, `[Key]`, `[ForeignKey]`) y `Microsoft.EntityFrameworkCore`. Esto acopla el dominio al ORM y contradice la afirmación del README de que Domain “no depende de ningún framework externo”.
- **Application contiene preocupaciones de infraestructura**: `AuthService` depende de `IConfiguration`, BCrypt y generación de JWT (`Application/Servicies/AuthService.cs`). El paquete `System.IdentityModel.Tokens.Jwt` y BCrypt están declarados en Application. `IConfiguration` se resuelve de forma transitiva (probablemente vía EF Core), lo que es frágil.
- **Infrastructure es la única capa con acceso a datos**, lo cual es correcto, pero las consultas mezclan LINQ sobre `DbSet` y SQL crudo (`FromSqlRaw`/`ExecuteSqlRaw`) sin un patrón uniforme.
- **WebApp registra los servicios manualmente** en `Program.cs` con `AddScoped`, correcto, pero no hay validación de configuración al arranque ni filtros/políticas globales.
- **No hay capa de dominio rica**: las entidades son anémicas y las reglas de negocio están en servicios, algunas de ellas como validaciones simples.

### 3.2 Patrones implementados

| Patrón | Estado | Observación |
|---|---|---|
| Repository | Implementado | Interfaces en Application; implementaciones en Infrastructure |
| DTO | Implementado parcialmente | Mapeo manual en servicios; varios DTOs sin uso y controladores que reciben entidades directamente |
| Dependency Injection | Implementado | Registro `Scoped` en `Program.cs` |
| Service layer | Implementado | `Application/Servicies`; en gran parte es pass-through al repositorio |
| Clean Architecture | Parcial | Dirección de dependencias correcta en lo esencial, con fugas de EF en Domain y de infraestructura en Application |
| Manejo de errores | Ausente | No hay middleware, filtros ni logging de excepciones; se propagan a la página de error |
| Transacciones | Ausentes | Crear artículo + imágenes no es atómico |
| Validación | Ausente en servidor | No hay anotaciones de validación en DTOs |

### 3.3 Evaluación de responsabilidades

- Los controladores de `Articulo`, `Favorito` y `Usuario` usan servicios (correcto).
- `CategoriaController` y `ReporteController` reciben y pasan entidades de dominio directamente, sin DTOs.
- `ImagenArticuloController` expone una API sin autenticación y sin DTOs.
- La lógica de negocio real es mínima: validaciones de IDs y campos obligatorios; no hay reglas de propiedad, estado, límites, duplicados ni moderación.
- El acceso a datos usa stored procedures para escrituras y lecturas mixtas; el contrato SQL no está documentado en el repositorio.

**Conclusión arquitectónica:** la base es razonable para un proyecto académico, pero no cumple estrictamente Clean Architecture y carece de las capas transversales (autorización, validación, errores, logging, transacciones) necesarias para producción. **No se recomienda un refactor masivo**; se recomienda corregir las fugas y añadir los componentes transversales de forma incremental.

---

## 4. Funcionalidades existentes (verificadas en código)

| # | Funcionalidad | Propósito | Estado real | Archivos relevantes | Dependencias | Problemas conocidos |
|---|---|---|---|---|---|---|
| F-01 | Landing page | Presentación del marketplace | Implementada (estática) | `Views/Home/Index.cshtml`, `wwwroot/css/landing.css`, `wwwroot/js/landing.js` | Ninguna | CTA y enlaces `#` no funcionales; suscripción solo muestra `alert` |
| F-02 | Registro | Crear cuenta con hash BCrypt y rol `usuario` | Implementado con fallos | `AuthController`, `AuthService`, `UsuarioRepository`, `Views/Auth/Registrar.cshtml` | SQL Server, SP `sp_insertar_usuario`, tablas `roles`/`usuario_roles` | Sin validación de servidor; sin política de contraseña; rol 2 hardcodeado; script `register.js` no coincide con `Registrar.js` |
| F-03 | Login / Logout | Autenticación JWT en cookie HttpOnly | Implementado con fallos | `AuthController`, `AuthService`, `Program.cs` | SQL Server, configuración JWT | Cookie `Secure=false`; no verifica usuario activo; login con token vencido redirige por existencia de cookie |
| F-04 | Catálogo / Dashboard | Listar artículos y filtrar en cliente | Implementado parcialmente | `DashboardController`, `ArticuloService`, `ArticuloRepository`, `Views/Dashboard/Index.cshtml` | SQL Server | Filtro por categoría nunca coincide; filtros de estado/precio inconsistentes; XSS almacenado; sin paginación |
| F-05 | Detalle de artículo | Galería, vendedor, WhatsApp, favorito, reporte | Implementado | `ArticuloController.Detalle`, `ArticuloService.ObtenerPorId`, `Views/Articulo/Detalle.cshtml` | SQL Server | Rating y “vendedor verificado” hardcodeados; WhatsApp sin teléfono real |
| F-06 | Publicar artículo | Crear artículo con múltiples imágenes | Implementado parcialmente | `ArticuloController.Crear`, `ArticuloService.CrearArticulos`, `Views/Articulo/Crear.cshtml` | SP `sp_insertar_articulo`, `sp_insertar_imagen_articulo`, disco local | Sin validación de archivos; ruta `imagen` vs `Imagen`; sin transacción; `imagenes` puede ser nulo; carpeta no se crea |
| F-07 | Cambiar estado de artículo | Disponible / reservado / vendido | Implementado en perfil; endpoint general roto | `UsuarioController.CambiarEstadoArticulo`, `ArticuloController.CambiarEstado`, `Views/Usuario/Perfil.cshtml` | SP `sp_cambiar_estado_articulo` | Sin verificación de propiedad; vista GET de `Articulo/CambiarEstado` ausente |
| F-08 | Eliminar artículo | Borrado por SP | Backend implementado; UI rota | `ArticuloController.Eliminar/EliminarConfirmado`, `ArticuloRepository.Eliminar` | SP `sp_eliminar_articulo` | Sin verificación de propiedad; vistas `Eliminar` ausentes; no borra archivos físicos |
| F-09 | Favoritos | Agregar, listar y quitar favoritos | Implementado | `FavoritoController`, `FavoritoService`, `FavoritoRepository`, `Views/Favorito/Agregar.cshtml` | SP `sp_agregar_fav`, `sp_eliminar_favorito`, índice único | Duplicado produce error 500; no hay verificación adicional |
| F-10 | Perfil | Ver perfil y artículos propios | Implementado parcialmente | `UsuarioController.Perfil`, `UsuarioService`, `Views/Usuario/Perfil.cshtml` | SQL Server | La foto de perfil nunca se muestra (DTO no la mapea) |
| F-11 | Editar perfil | Editar nombre, teléfono, ubicación y foto | Implementado con fallos | `UsuarioController.EditarPerfil`, `Views/Usuario/EditarPerfil.cshtml` | SP `sp_actualizar_usuario`, disco local | IDOR por `IdUsuario` oculto; borra la foto si no se sube una nueva; no muestra foto actual; sin validación de archivo |
| F-12 | Gestión de categorías | CRUD de categorías | Backend implementado; UI inexistente | `CategoriaController`, `CategoriaService`, `CategoriaRepository` | SP de categorías | Sin vistas; sin autorización; sin validación de duplicados |
| F-13 | Reportes | Crear y listar reportes | Backend parcial; UI inexistente | `ReporteController`, `ReporteService`, `ReporteRepository` | SP `sp_crear_reporte`, `sp_obtener_reportes` | Crear no asigna `IdUsuario`; vista ausente; listado expuesto sin autorización |
| F-14 | Administración de usuarios | Listar, crear, editar, eliminar usuarios | Roto | `UsuarioController.Index/Crear/Editar/Eliminar`, `UsuarioRepository` | SP de usuarios | `ObtenerTodos` ejecuta `sp_insertar_usuario`; vistas ausentes; sin autorización; mass assignment |
| F-15 | API de imágenes | GET/POST de imágenes de artículo | Implementada sin seguridad | `ImagenArticuloController` | SP `sp_obtener_imagenes_articulo`, `sp_insertar_imagen_articulo` | Sin autenticación, sin antiforgery, acepta URL arbitraria |
| F-16 | Cambio de estado desde perfil | Cambiar estado de artículos propios | Implementado | `UsuarioController.CambiarEstadoArticulo` | SP `sp_cambiar_estado_articulo` | No verifica que el artículo pertenezca al usuario |

### Funcionalidades ausentes o incompletas

- Edición de artículos: el README la menciona, pero no existe acción ni vista.
- Búsqueda/filtros reales en servidor: los filtros son client-side y algunos no funcionan.
- Panel de moderación/admin: no existe, aunque hay roles y reportes.
- Recuperación de contraseña, verificación de correo, notificaciones.
- Paginación.
- Eliminación de imágenes huérfanas.

---

## 5. Bugs y problemas funcionales identificados

### 5.1 CRITICAL

#### C-01 — Autorización inexistente en toda la aplicación
- **Archivos:** todos los controladores (`Vintaloo.WebApp/Controllers/*.cs`), `Program.cs`.
- **Descripción:** no hay atributos `[Authorize]`, políticas ni filtros globales. Solo `ArticuloController.Crear`, `FavoritoController`, `UsuarioController.Perfil/EditarPerfil` consultan manualmente el claim. El resto de acciones son accesibles sin autenticación.
- **Causa raíz:** la autenticación se configuró, pero la autorización se dejó manual y parcial.
- **Impacto:** cualquier usuario (incluso anónimo) puede invocar `Usuario/Index`, `Usuario/Eliminar`, `Categoria/*`, `Reporte/Index`, `Articulo/EliminarConfirmado`, etc.
- **Corrección recomendada:** añadir `[Authorize]` global o por controlador, políticas por rol y un helper/filtro de propiedad. Mantener redirecciones a login para MVC y 401/403 para API.

#### C-02 — IDOR en edición de perfil y administración de usuarios
- **Archivos:** `Vintaloo.WebApp/Controllers/UsuarioController.cs:192-220`.
- **Descripción:** el POST de `EditarPerfil` usa `dto.IdUsuario` recibido del formulario, sin compararlo con el claim del usuario autenticado. `Editar`, `Eliminar` y `Crear` tampoco tienen autorización.
- **Causa raíz:** se confía en el identificador enviado por el cliente.
- **Impacto:** un atacante puede editar el perfil de cualquier usuario, incluido nombre, teléfono y ubicación; en los endpoints administrativos podría eliminar usuarios.
- **Corrección recomendada:** obtener siempre el ID del claim; ignorar `IdUsuario` del formulario; restringir administración a rol admin.

#### C-03 — Cambio de estado y eliminación de artículos sin verificar propiedad
- **Archivos:** `Vintaloo.WebApp/Controllers/ArticuloController.cs:130-168`, `UsuarioController.cs:236-247`.
- **Descripción:** `CambiarEstado` y `EliminarConfirmado` operan por `id` sin comprobar que el artículo pertenezca al usuario actual ni que exista.
- **Causa raíz:** el servicio/repositorio no recibe el usuario y no hay regla de propiedad.
- **Impacto:** cualquier persona puede marcar como vendido o eliminar artículos ajenos.
- **Corrección recomendada:** cargar el artículo y validar `IdUsuario == usuarioActual` o rol admin antes de escribir.

#### C-04 — Esquema de base de datos y stored procedures no versionados
- **Archivos:** `README.md`, `Infrastructure/Repositories/*.cs`, `Infrastructure/Data/VintalooDbContext.cs`.
- **Descripción:** el repositorio referencia 17 stored procedures y varias tablas, pero no incluye script DDL/SP. El README apunta a `docs/database/vintaloo_schema.sql`, que no existe.
- **Causa raíz:** el esquema se gestionó fuera del repositorio.
- **Impacto:** imposible provisionar un entorno nuevo, reproducir fallos o desplegar; bloquea pruebas de integración y CI/CD.
- **Corrección recomendada:** versionar un script idempotente de tablas, índices, constraints, roles y stored procedures; documentar el contrato exacto de cada SP (parámetros, columnas devueltas y errores).

#### C-05 — Cadena de conexión hardcodeada que prevalece sobre la configuración
- **Archivos:** `Infrastructure/Data/VintalooDbContext.cs:35-37`, `Vintaloo.WebApp/appsettings.json:10`.
- **Descripción:** `OnConfiguring` llama a `UseSqlServer` con una cadena fija sin comprobar `optionsBuilder.IsConfigured`. La cadena también está en `appsettings.json`.
- **Causa raíz:** configuración duplicada tras el scaffolding.
- **Impacto:** cambiar la base de datos por entorno no surte efecto sin recompilar; riesgo de apuntar a la base local en producción.
- **Corrección recomendada:** eliminar `OnConfiguring` o condicionarlo con `IsConfigured`; usar solo `AddDbContext` y configuración por entorno/variables.

### 5.2 HIGH

#### H-01 — Listado de usuarios ejecuta un stored procedure de inserción
- **Archivo:** `Infrastructure/Repositories/UsuarioRepository.cs:26-31`.
- **Descripción:** `ObtenerTodos()` ejecuta `EXEC sp_insertar_usuario`, no una consulta de selección.
- **Impacto:** error de SQL o inserción de un usuario inválido; `Usuario/Index` no puede funcionar.
- **Corrección:** usar `sp_obtener_usuarios` o LINQ sobre `Usuarios`.

#### H-02 — Vistas ausentes para acciones existentes
- **Archivos:** `Views/Articulo/` (solo `Crear.cshtml` y `Detalle.cshtml`), `Views/Usuario/` (solo `Perfil` y `EditarPerfil`), no existen `Views/Categoria/` ni `Views/Reporte/`.
- **Acciones afectadas:** `Articulo/Index`, `Articulo/CambiarEstado` (GET), `Articulo/Eliminar` (GET); `Categoria/Index`, `Crear`, `Editar`, `Eliminar`; `Reporte/Index`, `Crear`; `Usuario/Index`, `Detalle`, `Crear`, `Editar`, `Eliminar`.
- **Impacto:** error 500 al invocar esas rutas; funcionalidades inaccesibles desde la UI.
- **Corrección:** implementar las vistas o retirar/ocultar las rutas hasta que existan. Decidir con el propietario cuáles forman parte del MVP.

#### H-03 — Creación de reportes no asigna usuario y no tiene vista
- **Archivo:** `Vintaloo.WebApp/Controllers/ReporteController.cs:34-40`.
- **Descripción:** el POST recibe `Reporte` directamente; `IdUsuario` queda en 0 y `ReporteService` lanza `ArgumentException`, no capturada. La vista `Reporte/Crear` no existe. El enlace de detalle pasa `idArticulo` por query string, pero el GET lo ignora.
- **Impacto:** reportar un anuncio produce error 500; no se puede moderar.
- **Corrección:** asignar el usuario del claim, usar un DTO, validar el artículo, crear la vista y manejar errores.

#### H-04 — La foto de perfil se pierde y nunca se muestra
- **Archivos:** `UsuarioController.cs:192-220`, `Application/Servicies/UsuarioService.cs` (`ObtenerUsuarioPorId`), `Views/Usuario/EditarPerfil.cshtml`, `Views/Usuario/Perfil.cshtml`.
- **Descripción:** `ObtenerUsuarioPorId` no mapea `FotoPerfil`, por lo que `Perfil` y `EditarPerfil` reciben null. Además, al guardar sin subir imagen, `usuario.FotoPerfil` queda null y se envía al SP, borrando la foto existente. `ViewBag.FotoActual` nunca se asigna.
- **Impacto:** las fotos de perfil no se visualizan y se eliminan al editar otros campos.
- **Corrección:** mapear `FotoPerfil`, recuperar la foto actual antes de actualizar y asignar `ViewBag.FotoActual`.

#### H-05 — XSS almacenado en el Dashboard
- **Archivo:** `Vintaloo.WebApp/Views/Dashboard/Index.cshtml:505,531-555`.
- **Descripción:** los artículos se serializan a JSON y se insertan en el DOM con `innerHTML` y template literals sin escapar `titulo`, `descripcion` ni `urlImagen`. Aunque `Json.Serialize` escape caracteres en el JSON, JavaScript los decodifica al construir el string y `innerHTML` los interpreta como HTML.
- **Impacto:** un vendedor puede inyectar HTML/JavaScript y ejecutarlo en el navegador de cualquier visitante (robo de sesión, acciones en nombre de otros).
- **Corrección:** construir nodos con `textContent`/DOM API o escapar HTML antes de `innerHTML`; además, sanear en servidor y aplicar CSP.

#### H-06 — Subida de archivos sin validación
- **Archivos:** `Vintaloo.WebApp/Controllers/ArticuloController.cs:83-97`, `UsuarioController.cs:207-219`, `Views/Articulo/Crear.cshtml:64`.
- **Descripción:** se usa la extensión del nombre original, no se valida MIME, contenido, tamaño ni cantidad. `accept="image/*"` es solo del cliente. Los archivos se sirven desde el mismo origen.
- **Impacto:** subida de archivos arbitrarios, agotamiento de disco, XSS almacenado mediante SVG/HTML y contenido malicioso.
- **Corrección:** validar extensión y firma binaria, limitar tamaño y cantidad, generar nombres seguros (ya se usa GUID), almacenar fuera de `wwwroot` o con extensión forzada, y servir con cabeceras seguras.

#### H-07 — Rutas de imágenes inconsistentes en mayúsculas/minúsculas
- **Archivos:** `ArticuloController.cs:89,96` (`wwwroot/imagen`, `/imagen/`), carpeta física `Vintaloo.WebApp/wwwroot/Imagen/`, `Views/Home/Index.cshtml` (`~/Imagen/logoVintaloo.png`).
- **Impacto:** en Windows funciona por insensibilidad del sistema de archivos; en Linux/contenedores las imágenes nuevas o existentes no se encuentran. Además, la carpeta `imagen` no se crea antes de escribir, por lo que la subida puede fallar.
- **Corrección:** unificar en una sola ruta, crear el directorio con `Directory.CreateDirectory` y usar una abstracción de almacenamiento configurable.

#### H-08 — API de imágenes sin autenticación ni antiforgery
- **Archivo:** `Vintaloo.WebApp/Controllers/ImagenArticuloController.cs:7-35`.
- **Descripción:** `POST /api/imagenes` acepta cualquier `ImagenesArticulo` y guarda una URL arbitraria; `GET /api/imagenes/{id}` es público.
- **Impacto:** cualquiera puede asociar imágenes a artículos ajenos o inyectar URLs externas; el endpoint no está protegido.
- **Corrección:** requerir autenticación, verificar propiedad del artículo, validar URL/archivo y aplicar antiforgery o token.

#### H-09 — Validación de servidor inexistente
- **Archivos:** `Application/DTOs/*.cs`, `Application/Servicies/*.cs`, controladores.
- **Descripción:** no hay `[Required]`, `[EmailAddress]`, `[StringLength]`, `[Range]` ni FluentValidation. `ModelState.IsValid` siempre es verdadero salvo errores de binding. Las validaciones de servicios son manuales e incompletas.
- **Impacto:** datos vacíos o inválidos pueden llegar a la base; mensajes de error dependen de excepciones SQL; la confirmación de contraseña solo existe en JavaScript.
- **Corrección:** anotar DTOs, validar en servidor, devolver errores de modelo y añadir reglas de negocio explícitas.

#### H-10 — Usuarios inactivos pueden iniciar sesión
- **Archivo:** `Application/Servicies/AuthService.cs` (`Login`).
- **Descripción:** no se consulta `usuario.Estado` antes de generar el token; además, `EditarPerfil` fuerza `Estado = "activo"`.
- **Impacto:** una cuenta desactivada sigue operando; editar el perfil reactiva cuentas desactivadas.
- **Corrección:** bloquear login si el estado no es `activo` y no sobrescribir el estado desde el flujo de perfil.

#### H-11 — Secreto JWT versionado y configuración sin gestión segura
- **Archivo:** `Vintaloo.WebApp/appsettings.json:13`.
- **Descripción:** la clave JWT está en el repositorio. No se usan User Secrets, variables de entorno ni Key Vault.
- **Impacto:** cualquiera con acceso al repositorio puede firmar tokens válidos.
- **Corrección:** rotar la clave, retirarla del repositorio, cargarla desde variables de entorno/secret manager y validar que exista al arranque.

#### H-12 — Cookie de autenticación sin `Secure`
- **Archivo:** `Vintaloo.WebApp/Controllers/AuthController.cs:46-51`.
- **Descripción:** `Secure = false` con el comentario explícito de cambiarlo en producción.
- **Impacto:** el token puede viajar por HTTP en texto claro.
- **Corrección:** `Secure = true` (o `CookieSecurePolicy.Always`) fuera de desarrollo; configurar `SameSite` y expiración desde configuración.

### 5.3 MEDIUM

#### M-01 — Sin manejo centralizado de errores ni logging
- **Archivos:** controladores y repositorios.
- **Descripción:** las excepciones de SQL, BCrypt y validaciones no se capturan; no hay middleware de excepciones, filtros ni logging estructurado. La página `Home/Error` no registra nada.
- **Impacto:** respuestas 500 genéricas, pérdida de trazabilidad y difícil diagnóstico en producción.
- **Corrección:** middleware de excepciones, logging estructurado, respuestas consistentes y registro de correlación.

#### M-02 — Creación de artículo e imágenes sin transacción
- **Archivos:** `Application/Servicies/ArticuloService.cs`, `ArticuloRepository`, `ImagenArticuloRepository`.
- **Descripción:** se inserta el artículo y luego cada imagen en llamadas independientes. Si una imagen falla, queda un artículo parcialmente creado.
- **Impacto:** datos huérfanos e inconsistentes.
- **Corrección:** transacción única o stored procedure que reciba el conjunto de imágenes.

#### M-03 — Favorito duplicado produce error 500
- **Archivos:** `FavoritoRepository.AgregarFavorito`, índice único `UQ_favoritos`.
- **Descripción:** no se comprueba duplicado antes de ejecutar el SP; la restricción única lanza una excepción SQL no controlada.
- **Impacto:** mala experiencia y error no controlado.
- **Corrección:** verificar existencia o capturar la violación de unicidad y mostrar un mensaje amable.

#### M-04 — Filtros del Dashboard incorrectos
- **Archivo:** `Views/Dashboard/Index.cshtml:501-516,583`.
- **Descripción:** `ArticuloDTO` no tiene propiedad `Categoria`, por lo que `a.categoria` es siempre `undefined` y el filtro por categoría nunca filtra. Los botones “Nuevo/Usado/Regular” no coinciden con los valores `Excelente/Bueno/Regular`. El slider inicia en 999999 y la etiqueta muestra 3000, mientras el máximo es 5 000 000.
- **Impacto:** filtros que aparentan funcionar pero no lo hacen.
- **Corrección:** incluir categoría en el DTO, alinear valores de estado y unificar el rango de precio.

#### M-05 — `imagenes` puede ser nulo en la creación de artículos
- **Archivo:** `ArticuloController.cs:83`.
- **Descripción:** el parámetro `List<IFormFile>` no está protegido contra null antes del `foreach`.
- **Impacto:** `NullReferenceException` si el formulario se envía sin archivos.
- **Corrección:** usar `imagenes ?? new List<IFormFile>()` y validar cantidad/mínimo.

#### M-06 — Carga completa de artículos sin paginación
- **Archivos:** `ArticuloRepository.obtenerArticulos`, `ArticuloService.ObtenerTodosArticulos`, `Dashboard/Index.cshtml`.
- **Descripción:** se cargan todos los artículos con todas sus imágenes y se serializan al cliente.
- **Impacto:** degradación progresiva de memoria, tiempo de respuesta y tamaño de HTML/JSON.
- **Corrección:** paginación en servidor, proyección directa a DTO y `AsNoTracking`.

#### M-07 — `sp_obtener_reportes` y otras lecturas sin verificación de columnas
- **Archivos:** `ReporteRepository`, `CategoriaRepository`, `ImagenArticuloRepository`, `UsuarioRepository`.
- **Descripción:** `FromSqlRaw` materializa entidades completas; si el SP no devuelve todas las columnas esperadas por EF, la consulta falla.
- **Impacto:** errores en tiempo de ejecución no detectables en compilación.
- **Corrección:** documentar y probar el contrato de cada SP; preferir proyecciones/DTOs.

#### M-08 — `Usuario/Crear` y `Usuario/Editar` aceptan entidades completas
- **Archivo:** `Vintaloo.WebApp/Controllers/UsuarioController.cs`.
- **Descripción:** el model binding permite asignar `PasswordHash`, `Estado`, `Correo`, etc. No se aplica hash en el flujo de creación administrativa.
- **Impacto:** mass assignment y almacenamiento de contraseñas sin hash.
- **Corrección:** usar DTOs específicos y servicios con reglas de creación/actualización.

#### M-09 — Roles definidos pero nunca aplicados
- **Archivos:** `AuthService`, `UsuarioRepository`, `Program.cs`.
- **Descripción:** se asigna el rol `usuario` y se emite el claim `Role`, pero ninguna acción exige rol. Las tablas `roles`/`usuario_roles` no tienen entidades y no hay flujo administrativo.
- **Impacto:** la moderación y administración no pueden protegerse; cualquier usuario podría ejecutar acciones de admin si se habilitan las vistas.
- **Corrección:** políticas por rol, entidades o repositorio de roles y endpoints administrativos protegidos.

#### M-10 — Dependencia de EF Core en Domain
- **Archivo:** `Domain/Domain.csproj:10-11`.
- **Descripción:** las entidades usan atributos de EF y el proyecto referencia EF Core.
- **Impacto:** acoplamiento del dominio al ORM; dificulta pruebas puras y migración tecnológica.
- **Corrección:** mover configuración a `OnModelCreating`/clases de configuración en Infrastructure y retirar EF de Domain (sin refactor masivo, de forma incremental).

#### M-11 — Cobertura de configuración incompleta
- **Archivo:** `Program.cs:42`.
- **Descripción:** `builder.Configuration["Jwt:Key"]!` usa null-forgiving; no hay validación al arranque. `ExpiresInHours` se parsea en `AuthService` y puede fallar.
- **Impacto:** error de arranque poco claro si falta configuración; expiración inconsistente entre cookie (8 h fijas) y token (config).
- **Corrección:** usar Options con `ValidateOnStart`, una sola fuente de expiración y mensajes claros.

#### M-12 — Archivos subidos versionados y sin limpieza
- **Archivos:** `Vintaloo.WebApp/wwwroot/Imagen/`, `wwwroot/perfiles/`, `.gitignore`.
- **Descripción:** hay imágenes de usuarios en el repositorio y `.gitignore` no excluye nuevas subidas; eliminar un artículo no elimina sus archivos.
- **Impacto:** repositorio pesado, datos personales versionados, disco lleno con huérfanos.
- **Corrección:** excluir uploads del control de versiones, almacenar fuera del árbol público y limpiar al eliminar.

#### M-13 — Sin rate limiting ni bloqueo de intentos de login
- **Archivo:** `Program.cs`, `AuthController`.
- **Descripción:** no hay límite de intentos ni retardo.
- **Impacto:** fuerza bruta de credenciales.
- **Corrección:** rate limiting, bloqueo temporal y registro de intentos fallidos.

#### M-14 — Sin cabeceras de seguridad ni CSP
- **Archivo:** `Program.cs`.
- **Descripción:** no se añaden `Content-Security-Policy`, `X-Content-Type-Options`, `Referrer-Policy`, etc. `AllowedHosts` es `*`.
- **Impacto:** mayor superficie para XSS/clickjacking y menor defensa en profundidad.
- **Corrección:** middleware de cabeceras, CSP compatible con los estilos/scripts actuales, allowed hosts por entorno.

#### M-15 — Ruta de script de registro incorrecta
- **Archivos:** `Views/Auth/Registrar.cshtml:445` (`~/js/register.js`), archivo real `wwwroot/js/Registrar.js`.
- **Impacto:** la validación de cliente no se carga en sistemas de archivos sensibles a mayúsculas (Linux/contenedores). En Windows puede funcionar por casualidad.
- **Corrección:** unificar nombre y referencia.

#### M-16 — `Reporte/Index` expone todos los reportes y no hay rol moderador
- **Archivo:** `ReporteController.Index`.
- **Impacto:** fuga de información y ausencia de control de moderación.
- **Corrección:** restringir a administradores y añadir vista/panel.

#### M-17 — `Articulo/Crear` usa `Directory.GetCurrentDirectory()`
- **Archivo:** `ArticuloController.cs:88`.
- **Descripción:** la ruta de escritura depende del directorio de trabajo del proceso.
- **Impacto:** comportamiento distinto entre `dotnet run`, IIS y contenedores; puede escribir fuera del sitio o fallar.
- **Corrección:** usar `IWebHostEnvironment.ContentRootPath`/`WebRootPath` y una abstracción de almacenamiento.

### 5.4 LOW

- **L-01:** enlaces `#` y CTA de suscripción sin backend (`Views/Home/Index.cshtml`, `wwwroot/js/landing.js`).
- **L-02:** DTOs sin uso (`CategoriaDTO`, `FavoritoDTO`, `ReporteDTO`, `CreateUsuarioDTO`, `UpdateUsuarioDTO`).
- **L-03:** inconsistencias de nombres/namespaces: `Application/Servicies` (error ortográfico), `Vintaloo_WebApp.src.Infrastructure.Repositories`, `Presentation.Controllers` para `ImagenArticuloController`.
- **L-04:** HTML inválido: `@Html.AntiForgeryToken()` después de `</body>` en `Dashboard/Index.cshtml`; llave `}` sobrante al final de `Views/Articulo/Crear.cshtml`; comentarios CSS que desactivan `@keyframes` en varias vistas.
- **L-05:** rating 4.9 y “Vendedor verificado” hardcodeados en `Views/Articulo/Detalle.cshtml`.
- **L-06:** la sesión guarda el correo en la clave `UsuarioNombre` y no se usa en ninguna vista (`AuthController.cs:54`).
- **L-07:** `Detalle.cshtml` comprueba `VendedorFoto != null`, pero un string vacío genera `<img src="">`.
- **L-08:** `Console.ReadKey()` en el `catch` de `Program.cs` puede bloquear entornos no interactivos.
- **L-09:** `error_inicio.txt` se escribe en el directorio base de la aplicación.
- **L-10:** `Auth/Login` redirige al Dashboard si existe cualquier cookie `jwt_token`, incluso vencida o inválida.
- **L-11:** README desactualizado y enlaces de imágenes a `docs/images/...` desde la raíz, donde no existen (las imágenes están en `Vintaloo.WebApp/docs/images`).
- **L-12:** falta la funcionalidad de edición de artículos anunciada en el README.
- **L-13:** dependencia de Google Fonts vía CDN (disponibilidad/privacidad).
- **L-14:** `appsettings.json` incluye `AllowedHosts: "*"` sin restricción por entorno.

---

## 6. Hallazgos de seguridad

| ID | Severidad | Hallazgo | Evidencia | Estado |
|---|---|---|---|---|
| S-01 | CRITICAL | Ausencia de autorización global y por recurso | Controladores sin `[Authorize]` | Verificado |
| S-02 | CRITICAL | IDOR en perfil y administración de usuarios | `UsuarioController.cs:192-220` | Verificado |
| S-03 | CRITICAL | Modificación/eliminación de artículos ajenos | `ArticuloController.cs:130-168` | Verificado |
| S-04 | HIGH | Secreto JWT versionado | `appsettings.json:13` | Verificado |
| S-05 | HIGH | Cookie de autenticación sin `Secure` | `AuthController.cs:48` | Verificado |
| S-06 | HIGH | XSS almacenado en el catálogo | `Dashboard/Index.cshtml:531-555` | Verificado por análisis |
| S-07 | HIGH | Subida de archivos sin validación | `ArticuloController.cs:83-97`, `UsuarioController.cs:207-219` | Verificado |
| S-08 | HIGH | API de imágenes pública y sin antiforgery | `ImagenArticuloController.cs` | Verificado |
| S-09 | HIGH | Validación de servidor inexistente | DTOs sin anotaciones | Verificado |
| S-10 | HIGH | Usuarios inactivos pueden autenticarse | `AuthService.Login` | Verificado |
| S-11 | MEDIUM | Fuerza bruta sin límites | `Program.cs`, `AuthController` | Verificado |
| S-12 | MEDIUM | Sin cabeceras de seguridad/CSP | `Program.cs` | Verificado |
| S-13 | MEDIUM | Mass assignment en entidades de usuario | `UsuarioController.Crear/Editar` | Verificado |
| S-14 | MEDIUM | Reportes/listados administrativos expuestos | `ReporteController.Index`, `UsuarioController.Index` | Verificado |
| S-15 | LOW | Enlaces externos y contenido hardcodeado sin validación | vistas | Verificado |

### Evaluación específica

- **Inyección SQL:** no se encontró concatenación de entrada de usuario en SQL. Las llamadas usan placeholders (`{0}`) con `FromSqlRaw`/`ExecuteSqlRaw`, lo que es correcto. El contenido interno de los stored procedures no es auditable porque no está en el repositorio (**no verificable**).
- **CSRF:** las acciones POST de MVC usan `[ValidateAntiForgeryToken]` y los formularios incluyen el token, lo cual es correcto. La API `POST /api/imagenes` no tiene antiforgery ni autenticación (ver S-08).
- **XSS:** Razor escapa la mayoría de salidas (`@Model...`), pero el renderizado con `innerHTML` del Dashboard rompe esa protección y los archivos subidos se sirven desde el mismo origen.
- **Secretos:** hay una clave JWT y una cadena de conexión en el repositorio. No se encontraron contraseñas de base de datos (autenticación integrada de Windows).
- **Autorización:** el claim de rol se emite pero no se usa; no hay separación usuario/admin.

---

## 7. Deuda técnica

1. **Contrato de base de datos no versionado** (tablas, índices, roles y 17 stored procedures).
2. **Configuración duplicada y acoplada** (cadena de conexión en dos lugares; secreto en repositorio).
3. **Ausencia de validación y de manejo de errores** en toda la pila.
4. **Duplicación de vistas/estilos**: cada vista usa `Layout = null` y repite navbar, estilos y estructura; difícil mantenimiento y consistencia.
5. **Mezcla de acceso a datos**: LINQ y SQL crudo sin criterio uniforme; entidades expuestas en controladores.
6. **Domain acoplado a EF Core**; Application con dependencias de infraestructura.
7. **Sin transacciones** en operaciones compuestas.
8. **Sin paginación ni proyecciones optimizadas**.
9. **Recursos estáticos y uploads mezclados con el código**, con rutas sensibles a mayúsculas.
10. **Código muerto y nombres inconsistentes** (`Servicies`, DTOs sin uso, namespaces heredados).
11. **Documentación desalineada** con la implementación real (README).
12. **Advertencias de nulabilidad** en llamadas SQL que pueden ocultar errores de parámetros nulos.

---

## 8. Pruebas faltantes

**Estado actual:** no existe ningún proyecto de pruebas, ni unitarias, ni de integración, ni E2E. Tampoco hay cobertura ni CI.

Áreas críticas sin pruebas:

- Autenticación: login válido/inválido, usuario inactivo, expiración y validación de JWT, logout.
- Registro: correo duplicado, contraseña débil, datos inválidos, asignación de rol.
- Autorización: acceso anónimo, acceso cruzado entre usuarios, acciones administrativas.
- Artículos: creación con/sin imágenes, validación de archivos, cambio de estado, eliminación y propiedad.
- Favoritos: agregar, duplicado, eliminar, aislamiento por usuario.
- Perfil: actualización parcial, conservación de foto, IDOR.
- Reportes: creación con usuario correcto, listado restringido.
- Repositorios: contrato de cada stored procedure contra una base de pruebas.
- Vistas/rutas: todas las acciones deben renderizar o devolver el código esperado.
- Seguridad: XSS, subida de archivos, CSRF, cookies.

**Recomendación (Fase 2):** xUnit + Moq o NSubstitute para unitarias; `WebApplicationFactory` para integración; SQL Server en contenedor/LocalDB para integración de datos; Playwright para flujos reales; Coverlet para cobertura. No añadir dependencias hasta aprobar la estrategia.

---

## 9. Bloqueadores de producción

1. **Sin esquema ni stored procedures versionados** → no se puede provisionar ni desplegar.
2. **Sin autorización efectiva** → riesgo crítico de integridad y datos.
3. **Secretos y cadena de conexión en el repositorio/código** → riesgo de seguridad y configuración.
4. **Almacenamiento de imágenes en disco local** → se pierde en contenedores/PaaS y no escala horizontalmente.
5. **Rutas de imágenes sensibles a mayúsculas** → imágenes rotas en Linux.
6. **Sin manejo de errores, logging, health checks ni observabilidad** → operación no soportable.
7. **Sin pruebas ni CI/CD** → despliegues sin validación.
8. **Cookie sin `Secure` y sin cabeceras de seguridad** → postura insegura.
9. **Sin estrategia de migración/backup de base de datos**.
10. **Sin paginación** → degradación con datos reales.

---

## 10. Roadmap recomendado

> El orden busca estabilizar antes de añadir funcionalidad. Cada fase requiere validación y aprobación antes de avanzar.

### Fase 1 — Estabilización crítica (bloqueante)
1. Implementar autorización global y por recurso (`[Authorize]`, políticas, verificación de propiedad).
2. Corregir IDOR en perfil, usuarios, artículos, categorías, reportes e imágenes.
3. Sacar secretos y cadena de conexión del código; configurar por entorno; rotar la clave JWT.
4. Versionar esquema, índices, roles y stored procedures; documentar contratos.
5. Añadir validación de servidor en DTOs y servicios; validar subidas.
6. Corregir `ObtenerTodos`, rutas/vistas ausentes y creación de reportes.
7. Configurar cookie segura y flags de seguridad básicos.

### Fase 2 — Corrección funcional y consistencia
1. Recuperar foto de perfil y evitar su borrado.
2. Corregir filtros del Dashboard y unificar rutas de imágenes.
3. Manejo centralizado de errores, transacciones y duplicados de favoritos.
4. Limpiar código muerto, nombres y HTML inválido.
5. Completar o retirar funcionalidades sin UI (categorías, reportes, administración).

### Fase 3 — Pruebas automatizadas
1. Unitarias de servicios y autenticación.
2. Integración de controladores, DI y repositorios con base aislada.
3. E2E de registro, login, publicación, favoritos, perfil y moderación.
4. Cobertura y documentación en `docs/TESTING_STRATEGY.md`.

### Fase 4 — Preparación para producción
1. Logging estructurado, health checks y respuestas de error seguras.
2. Rate limiting, cabeceras de seguridad y CSP.
3. Paginación, proyecciones y optimización de consultas.
4. Almacenamiento persistente de imágenes (a definir con el propietario).
5. Estrategia de backup y migración de base de datos.

### Fase 5 — CI/CD
1. Pipeline de restore, build Release, tests, cobertura y publish.
2. Gestión segura de secretos.
3. Sin despliegues automáticos hasta aprobación explícita.

### Fase 6 — Despliegue
1. Comparativa de proveedores y aprobación.
2. Configuración de producción, HTTPS y base de datos.
3. Verificación real de la URL y flujos principales.

### Fase 7 — Documentación final
1. Actualizar `README.md` con la implementación real.
2. Añadir `docs/BUG_FIX_REPORT.md`, `docs/TESTING_STRATEGY.md`, `docs/DEPLOYMENT_GUIDE.md` y `docs/PRODUCTION_READINESS_REPORT.md` según avance.

---

## 11. Decisiones que requieren aprobación antes de la Fase 1

1. **Alcance del MVP:** ¿las funcionalidades sin vistas (categorías, reportes, administración de usuarios) deben completarse ahora o retirarse temporalmente? Recomendación: retirar las rutas no usadas del MVP y completar solo lo necesario.
2. **Modelo de roles:** ¿se implementará un rol administrador/moderador real para categorías, reportes y usuarios? Recomendación: sí, con políticas y un panel mínimo.
3. **Almacenamiento de imágenes:** ¿se mantendrá disco local para el MVP o se migrará a almacenamiento persistente (por ejemplo, blob/S3) antes del despliegue? Recomendación: abstraer el almacenamiento ahora y decidir el proveedor en la Fase 4.
4. **Base de datos:** ¿se proporcionará el script real de `vintaloo_db` y los stored procedures para versionarlo, o se autoriza reconstruirlo a partir de las entidades? Recomendación: versionar el script existente, no reconstruirlo a ciegas.
5. **Comportamiento del catálogo:** ¿el Dashboard debe mostrar artículos vendidos/reservados o solo disponibles? El código actual los muestra todos.
6. **Registro:** ¿se requiere verificación de correo y política mínima de contraseña, o basta con la validación básica para el MVP?

---

## 12. Método, comandos y limitaciones

**Inspeccionado:** solución y proyectos, dependencias, entidades, DbContext, interfaces, DTOs, servicios, repositorios, controladores, vistas Razor, JavaScript, CSS, configuración, `.gitignore`, README, estado de Git, estructura de carpetas, pruebas/CI/Docker y referencias a stored procedures.

**Comandos ejecutados (solo lectura/validación):**
- `git status`, `git branch --show-current`, `git log --oneline -10`, `git remote -v`, `git diff`.
- Listados de archivos y búsquedas con PowerShell/Select-String.
- `dotnet build Vintaloo.WebApp.sln --nologo` (0 errores, 14 advertencias).

**Estado de Git observado:** rama `main`; cambio no confirmado en `Vintaloo.WebApp/Vintaloo.WebApp.sln`; archivos no rastreados `AGENTS.md` y `VINTALOO_ROADMAP.md`.

**Limitaciones:**
- No se ejecutó la aplicación ni se conectó a SQL Server; no se validaron los stored procedures en ejecución.
- No se probaron flujos en navegador.
- El contenido de los stored procedures no está en el repositorio y no pudo auditarse.
- Los hallazgos de XSS y comportamiento de EF se basan en análisis estático.

**Cambios realizados en el código de aplicación:** ninguno. Este documento es el único artefacto creado.
