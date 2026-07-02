# 🛍️ Vintaloo WebApp

> **Tu mercado retro-contemporáneo** — Plataforma de compraventa de artículos únicos, vintage y de segunda mano en Costa Rica, construida con Clean Architecture sobre ASP.NET Core 8 y SQL Server.

<br>

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity_Framework-8.0.5-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

![Status](https://img.shields.io/badge/Status-En_Desarrollo-yellow?style=flat-square)
![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue?style=flat-square)
![Pattern](https://img.shields.io/badge/Pattern-Repository_Pattern-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-lightgrey?style=flat-square)

---

## 📋 Tabla de Contenidos

- [Descripción General](#-descripción-general)  
- [Demo del Sistema](#-demo-del-sistema)
- [Características Principales](#-características-principales)
- [Arquitectura](#-arquitectura)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Modelo de Negocio](#-modelo-de-negocio)
- [Base de Datos](#-base-de-datos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Capturas de Pantalla](#-capturas-de-pantalla)
- [Roadmap](#-roadmap)
- [Buenas Prácticas Detectadas](#-buenas-prácticas-detectadas)
- [Aprendizajes Técnicos](#-aprendizajes-técnicos)
- [Autor](#-autor)

---

## 🌿 Descripción General

**Vintaloo** es una plataforma de marketplace enfocada en la economía circular costarricense. Conecta a compradores y vendedores de artículos únicos, vintage, artesanías y piezas de segunda mano, promoviendo el consumo responsable y la reducción de residuos en el Valle Central de Costa Rica.

### ¿Qué problema resuelve?

En Costa Rica no existe una plataforma local especializada en el mercado retro-vintage que combine:

- Una experiencia de usuario moderna y atractiva
- Un modelo de negocio orientado a la sostenibilidad ambiental
- Herramientas de moderación y confianza entre usuarios (reportes, favoritos)
- Una arquitectura técnica escalable y mantenible

### ¿Para quién fue creado?

- **Vendedores locales** que desean publicar artículos únicos o de segunda mano
- **Compradores** que buscan piezas con historia, artesanías o productos con impacto ambiental positivo
- **Comunidades locales** comprometidas con la economía circular en Costa Rica

### Valor Agregado

- 🌱 Reduce la huella de carbono promoviendo la reutilización de objetos
- 🇨🇷 Foco 100% en el mercado costarricense
- 🏗️ Base técnica sólida con Clean Architecture lista para escalar
- 🔒 Diseño preparado para autenticación y autorización robusta

---

## 🎬 Demo del Sistema

> ⚠️ **Próximamente** — Las capturas y demos serán agregadas conforme avanza el desarrollo.

```markdown
<!-- Para agregar un GIF demo -->
![Demo Vintaloo](docs/images/demo.gif)

<!-- Para agregar un video -->
[![Ver Demo](docs/images/thumbnail.png)](https://youtube.com/tu-link)
```

---

## ✨ Características Principales

| Funcionalidad | Descripción | Estado |
|--------------|-------------|--------|
| 🏠 **Landing Page** | Página de inicio con hero, sección de pasos, impacto sostenible y CTA de suscripción | ✅ Implementado |
| 👤 **Gestión de Usuarios** | Registro, edición de perfil, foto de perfil, ubicación, estado de cuenta | ✅ Repositorio listo |
| 📦 **Publicación de Artículos** | Crear, editar, eliminar publicaciones con título, descripción, precio, categoría y ubicación | ✅ Repositorio listo |
| 🖼️ **Imágenes de Artículos** | Soporte para múltiples imágenes por publicación (tabla `imagenes_articulos`) | ✅ Entidad modelada |
| 🗂️ **Categorías** | Clasificación de artículos por categoría con nombre y descripción | ✅ Repositorio listo |
| ❤️ **Sistema de Favoritos** | Los usuarios pueden guardar artículos favoritos con restricción de unicidad por usuario | ✅ Repositorio listo |
| 🚨 **Sistema de Reportes** | Reporte de publicaciones sospechosas o inapropiadas con motivo, descripción y estado | ✅ Repositorio listo |
| 🔄 **Cambio de Estado** | Gestión del estado de publicación (disponible, vendido, etc.) y estado del artículo | ✅ Implementado |
| 📊 **DTOs** | Capa de transferencia de datos con DTOs de lectura y creación para Artículo y Usuario | ✅ Implementado |
| 🗄️ **Stored Procedures** | Toda la lógica de acceso a datos encapsulada en stored procedures de SQL Server | ✅ Implementado |

---

## 🏛️ Arquitectura

Vintaloo implementa **Clean Architecture** (también conocida como Arquitectura en Capas), separando responsabilidades en cuatro proyectos independientes. Esta decisión arquitectónica garantiza que las reglas de negocio sean independientes de frameworks, bases de datos y UI.

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│              Vintaloo.WebApp (ASP.NET Core MVC)          │
│         Controllers · Views (Razor) · wwwroot            │
└───────────────────────┬─────────────────────────────────┘
                        │ depende de
┌───────────────────────▼─────────────────────────────────┐
│                  Infrastructure Layer                    │
│                  Infrastructure.csproj                   │
│       VintalooDbContext · Repositories · EF Core         │
└───────────────────────┬─────────────────────────────────┘
                        │ depende de
┌───────────────────────▼─────────────────────────────────┐
│                  Application Layer                       │
│                  Application.csproj                      │
│          Interfaces (Contratos) · DTOs · Services        │
└───────────────────────┬─────────────────────────────────┘
                        │ depende de
┌───────────────────────▼─────────────────────────────────┐
│                    Domain Layer                          │
│                    Domain.csproj                         │
│              Entities (Artículo, Usuario, etc.)          │
└─────────────────────────────────────────────────────────┘
```

### Responsabilidad de Cada Capa

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Domain** | `Domain.csproj` | Entidades del negocio, anotaciones de base de datos, relaciones entre modelos. No depende de ningún framework externo. |
| **Application** | `Application.csproj` | Define los contratos (interfaces de repositorios) y los DTOs. Es el núcleo de las reglas de negocio. |
| **Infrastructure** | `Infrastructure.csproj` | Implementa los contratos definidos en Application: `VintalooDbContext`, repositorios concretos, llamadas a stored procedures. |
| **Presentation** | `Vintaloo.WebApp` | Controladores MVC, vistas Razor, assets estáticos (CSS/JS), configuración de DI y pipeline HTTP. |

### Beneficios de esta Arquitectura

- **Testabilidad:** La capa Application define interfaces que pueden ser mockeadas en pruebas unitarias sin tocar la base de datos.
- **Mantenibilidad:** Cambiar el ORM o la base de datos solo impacta Infrastructure, no el resto del sistema.
- **Escalabilidad:** Cada capa puede crecer de forma independiente; agregar nuevos casos de uso no rompe capas adyacentes.
- **Separación de Concerns:** La lógica de presentación nunca mezcla lógica de acceso a datos.

---

## 🔧 Tecnologías Utilizadas

### Backend

| Tecnología | Versión | Uso |
|-----------|---------|-----|
| C# | 12.0 | Lenguaje principal |
| ASP.NET Core MVC | 8.0 | Framework web |
| Entity Framework Core | 8.0.5 | ORM / acceso a datos |
| EF Core Relational | 8.0.5 | Soporte para stored procedures con `FromSqlRaw` / `ExecuteSqlRaw` |
| EF Core Design | 8.0.5 | Scaffolding del DbContext (Database-First) |

### Frontend

| Tecnología | Versión | Uso |
|-----------|---------|-----|
| Razor Views (.cshtml) | — | Motor de plantillas del servidor |
| Bootstrap | 5.x | Sistema de grillas y componentes UI |
| CSS personalizado | — | `landing.css`, `layout.css`, estilos del hero, tarjetas y secciones |
| JavaScript vanilla | — | `landing.js` para interactividad de la landing page |
| jQuery | — | Utilidades DOM y validación |
| jQuery Validation | — | Validación de formularios en cliente |

### Base de Datos

| Tecnología | Uso |
|-----------|-----|
| SQL Server (Express) | Motor de base de datos relacional |
| Stored Procedures | Toda la lógica CRUD encapsulada: `sp_insertar_articulo`, `sp_obtener_articulos`, `sp_eliminar_articulo`, `sp_insertar_usuario`, `sp_actualizar_usuario`, etc. |

### Patrones y Arquitectura

| Patrón | Implementación |
|--------|---------------|
| Clean Architecture | Cuatro capas independientes con inversión de dependencias |
| Repository Pattern | `IArticuloRepository`, `IUsuarioRepository`, etc. |
| DTO Pattern | `ArticuloDTO`, `CreateArticuloDTO`, `UsuarioDTO`, `CreateUsuarioDTO` |
| Dependency Injection | Registro en `Program.cs` con `AddScoped` para todos los repositorios |

### Herramientas

| Herramienta | Uso |
|------------|-----|
| Visual Studio 2022 | IDE principal |
| Git / GitHub | Control de versiones |
| SQL Server Management Studio | Administración de base de datos |

---

## 📁 Estructura del Proyecto

```text
Vintalooo_WebApp/
│
├── Domain/                          # 🔵 Capa de Dominio
│   ├── Domain.csproj
│   └── Entities/
│       ├── Articulo.cs              # Entidad principal del marketplace
│       ├── Usuario.cs               # Usuarios registrados
│       ├── Categoria.cs             # Categorías de artículos
│       ├── Favorito.cs              # Artículos guardados por usuario
│       ├── Reporte.cs               # Reportes de publicaciones
│       └── ImagenesArticulo.cs      # Imágenes de cada artículo
│
├── Application/                     # 🟢 Capa de Aplicación
│   ├── Application.csproj
│   ├── Interfaces/
│   │   ├── IArticuloRepository.cs   # Contrato para artículos
│   │   ├── IUsuarioRepository.cs    # Contrato para usuarios
│   │   ├── ICategoriaRepository.cs  # Contrato para categorías
│   │   ├── IFavoritoRepository.cs   # Contrato para favoritos
│   │   ├── IReporteRepository.cs    # Contrato para reportes
│   │   └── IImagenArticuloRepository.cs
│   ├── DTOs/
│   │   ├── ArticuloDTO.cs           # DTO de lectura de artículos
│   │   ├── CreateArticuloDTO.cs     # DTO de creación de artículos
│   │   ├── UsuarioDTO.cs            # DTO de lectura de usuarios
│   │   ├── CreateUsuarioDTO.cs      # DTO de creación de usuarios
│   │   ├── FavoritoDTO.cs
│   │   └── ReporteDTO.cs
│   └── Servicies/                   # (Carpeta preparada para Services)
│
├── Infrastructure/                  # 🟠 Capa de Infraestructura
│   ├── Infrastructure.csproj
│   ├── Data/
│   │   └── VintalooDbContext.cs     # DbContext con fluent API y relaciones
│   └── Repositories/
│       ├── ArticuloRepository.cs    # CRUD via stored procedures
│       ├── UsuarioRepository.cs     # CRUD via stored procedures
│       ├── CategoriaRepository.cs
│       ├── FavoritoRepository.cs
│       ├── ReporteRepository.cs
│       └── ImagenArticuloRepository.cs
│
└── Vintaloo.WebApp/                 # 🔴 Capa de Presentación
    ├── Vintaloo.WebApp.csproj
    ├── Program.cs                   # Entry point, DI, pipeline HTTP
    ├── appsettings.json             # Configuración y connection string
    ├── Controllers/
    │   └── HomeController.cs        # Controlador inicial (Index, Privacy, Error)
    ├── Views/
    │   ├── Home/
    │   │   ├── Index.cshtml         # Landing page completa
    │   │   └── Privacy.cshtml
    │   └── Shared/
    │       ├── _Layout.cshtml       # Layout principal con navbar y footer
    │       ├── Error.cshtml
    │       └── _ValidationScriptsPartial.cshtml
    └── wwwroot/
        ├── css/
        │   ├── landing.css          # Estilos hero, pasos, impacto, CTA
        │   ├── layout.css           # Estilos del navbar y footer global
        │   └── site.css
        ├── js/
        │   ├── landing.js           # Interactividad de la landing
        │   └── site.js
        └── lib/                     # Bootstrap, jQuery, jQuery Validation
```

---

## 🏢 Modelo de Negocio

### Actores del Sistema

| Actor | Descripción |
|-------|-------------|
| **Vendedor** | Usuario registrado que publica artículos, gestiona sus publicaciones y responde a interesados |
| **Comprador** | Usuario registrado que navega, guarda favoritos y contacta vendedores |
| **Moderador** (futuro) | Administrador que revisa reportes y gestiona el estado de publicaciones |

### Flujo Principal

```
[Usuario] → Registro/Login
     ↓
[Comprador] Explorar artículos → Filtrar por categoría → Guardar en favoritos
     ↓
[Vendedor] Crear publicación → Subir imágenes → Definir precio y ubicación
     ↓
[Sistema] Artículo disponible en marketplace
     ↓
[Comprador] Contacto con vendedor (fuera de plataforma)
     ↓
[Sistema] Estado: disponible → vendido
     ↓
[Reporte] Si el artículo incumple normas → pendiente → revisado
```

### Casos de Uso Principales

- **UC-01:** Registrar nuevo usuario con hash de contraseña
- **UC-02:** Publicar artículo con imágenes, precio y categoría
- **UC-03:** Listar artículos disponibles del marketplace
- **UC-04:** Guardar artículo como favorito
- **UC-05:** Reportar publicación inapropiada
- **UC-06:** Actualizar estado de publicación (disponible / vendido)
- **UC-07:** Eliminar publicación propia

---

## 🗃️ Base de Datos

La base de datos `vintaloo_db` sigue un diseño Database-First con 6 tablas principales y acceso exclusivo mediante stored procedures.

### Diagrama Entidad-Relación

```
usuarios (1) ─────────────── (N) articulos
    │                               │
    │ (1)                           │ (1)
    │                               │
    └── (N) favoritos (N) ──────────┘
    │
    └── (N) reportes (N) ────── (1) articulos
                                    │
                            (N) imagenes_articulos
                                    │
                            (N) categorias (1) ───── articulos
```

### Tablas

| Tabla | Campos Clave | Descripción |
|-------|-------------|-------------|
| `usuarios` | `id_usuario`, `correo` (UNIQUE), `password_hash`, `estado` | Usuarios registrados con estado activo/inactivo |
| `articulos` | `id_articulo`, `id_usuario`, `id_categoria`, `precio`, `estado_publicacion` | Publicaciones del marketplace |
| `categorias` | `id_categoria`, `nombre` | Clasificación de artículos |
| `favoritos` | `id_favorito`, `id_usuario`, `id_articulo` (UNIQUE compuesto) | Wishlist del usuario |
| `reportes` | `id_reporte`, `motivo`, `estado` (default: `pendiente`) | Moderación de contenido |
| `imagenes_articulos` | `id_imagen`, `id_articulo`, `url_imagen` | Imágenes asociadas a una publicación |

### Stored Procedures Identificados

```sql
sp_obtener_articulos           -- Listar todos los artículos
sp_articulo_porID              -- Obtener artículo por ID
sp_insertar_articulo           -- Crear nueva publicación
sp_cambiar_estado_articulo     -- Cambiar estado de publicación
sp_eliminar_articulo           -- Eliminar publicación
sp_obtener_usuarios_porId      -- Obtener usuario por ID
sp_insertar_usuario            -- Registrar nuevo usuario
sp_actualizar_usuario          -- Actualizar perfil de usuario
sp_eliminar_usuario            -- Eliminar usuario
```

---

## ⚙️ Instalación

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) o SQL Server completo
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recomendado) o VS Code con extensión C#

### Paso a Paso

**1. Clonar el repositorio**
```bash
git clone https://github.com/Felix-alvx/vintaloo-webapp.git
cd vintaloo-webapp
```

**2. Crear la base de datos**

Abre SQL Server Management Studio y ejecuta el script de creación de la base de datos:
```sql
CREATE DATABASE vintaloo_db;
```
Luego ejecuta el script DDL con las tablas y stored procedures (ubicado en `docs/database/vintaloo_schema.sql`).

**3. Configurar la conexión**

Edita `Vintaloo.WebApp/appsettings.json` y actualiza el `ConnectionString` con tu instancia de SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR\\SQLEXPRESS;Database=vintaloo_db;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**4. Restaurar paquetes NuGet**
```bash
dotnet restore
```

**5. Compilar el proyecto**
```bash
dotnet build
```

**6. Ejecutar la aplicación**
```bash
cd Vintaloo.WebApp
dotnet run
```

La aplicación estará disponible en `https://localhost:7XXX` o `http://localhost:5XXX`.

---

## 🔧 Configuración

### `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR\\INSTANCIA;Database=vintaloo_db;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### `appsettings.Development.json`

Para desarrollo local, se puede sobreescribir el nivel de logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Registro de Dependencias (`Program.cs`)

Los repositorios están registrados con ciclo de vida `Scoped`, lo que garantiza que cada request HTTP obtiene su propia instancia:

```csharp
builder.Services.AddDbContext<VintalooDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<ArticuloRepository>();
builder.Services.AddScoped<ImagenArticuloRepository>();
builder.Services.AddScoped<FavoritoRepository>();
builder.Services.AddScoped<ReporteRepository>();
```

---

## 📸 Capturas de Pantalla

### Landing Page

```markdown
![Hero Section](docs/images/hero.png)
```

### Sección de Pasos

```markdown
![Tres Pasos](docs/images/pasos.png)
```

### Impacto Sostenible

```markdown
![Impacto](docs/images/impacto.png)
```

### Marketplace (próximamente)

```markdown
![Marketplace](docs/images/marketplace.png)
```

---

## 🗺️ Roadmap

### v1.0 — MVP (En curso)
- [x] Landing page con diseño visual propio
- [x] Arquitectura en capas (Clean Architecture)
- [x] Modelado completo de entidades de dominio
- [x] Implementación de repositorios con stored procedures
- [x] DTOs para transferencia de datos
- [ ] Script SQL completo con tablas y stored procedures
- [ ] Controladores para Artículo, Usuario, Categoría

### v1.1 — Autenticación
- [ ] Registro e inicio de sesión con JWT o ASP.NET Identity
- [ ] Hash seguro de contraseñas (BCrypt o Argon2)
- [ ] Middleware de autorización por roles

### v1.2 — Funcionalidades Core
- [ ] CRUD completo de artículos desde la UI
- [ ] Subida de imágenes a servidor o servicio de almacenamiento (Azure Blob / Cloudinary)
- [ ] Sistema de búsqueda y filtros por categoría, precio, ubicación
- [ ] Gestión de favoritos desde el frontend

### v1.3 — Moderación y Calidad
- [ ] Panel de administración para gestión de reportes
- [ ] Sistema de notificaciones (email o in-app)
- [ ] Paginación de listados

### v2.0 — Escalabilidad
- [ ] Migración a arquitectura de servicios (capa de Services entre Application e Infrastructure)
- [ ] API REST separada para soporte de app móvil
- [ ] Tests unitarios e integración
- [ ] Despliegue en Azure App Service + Azure SQL

---

## ✅ Buenas Prácticas Detectadas

### Repository Pattern
Las interfaces en `Application/Interfaces/` definen contratos abstractos (`IArticuloRepository`, `IUsuarioRepository`, etc.) que son implementados en `Infrastructure/Repositories/`. Esto desacopla la lógica de negocio del motor de base de datos.

### Dependency Injection
Los repositorios se registran en `Program.cs` mediante el contenedor de DI nativo de ASP.NET Core con `AddScoped`. Los controladores y servicios futuros recibirán sus dependencias sin instanciarlas manualmente.

### DTO Pattern
Los DTOs (`ArticuloDTO`, `CreateArticuloDTO`, `UsuarioDTO`, `CreateUsuarioDTO`) protegen el modelo de dominio de exponerse directamente en la capa de presentación o API, siguiendo el principio de Information Hiding.

### Stored Procedures
Toda la lógica SQL está encapsulada en stored procedures nombrados consistentemente (`sp_*`), centralizando el acceso a datos en la base de datos y separándolo del código de aplicación.

### Separation of Concerns
Cada proyecto del solution tiene una responsabilidad única y bien definida, siguiendo el principio SRP de SOLID a nivel de solución.

### Nullable Reference Types
El proyecto está configurado con `<Nullable>enable</Nullable>`, forzando el uso explícito de `?` para tipos de referencia anulables y reduciendo errores de `NullReferenceException` en tiempo de ejecución.

---

## 📈 Rendimiento y Escalabilidad

La arquitectura actual facilita la evolución del sistema en múltiples dimensiones:

**Mantenibilidad:** La separación estricta entre capas permite que un desarrollador modifique los repositorios sin entender la capa de presentación, y viceversa. Los stored procedures centralizan la lógica SQL y permiten optimizaciones de base de datos independientes del código C#.

**Testabilidad:** Al depender de interfaces (`IArticuloRepository`) en lugar de implementaciones concretas, es posible introducir mocks o fakes en pruebas unitarias sin levantar una base de datos real.

**Escalabilidad horizontal:** La capa `Application/Servicies/` (ya presente en la estructura) está preparada para recibir la capa de servicios que orqueste múltiples repositorios, transformaciones y reglas de negocio, desacoplando aún más el controlador de la base de datos.

**Evolución tecnológica:** Migrar de SQL Server a PostgreSQL solo requiere cambiar la implementación del DbContext en Infrastructure, sin tocar Domain, Application ni Presentation.

---

## 🎓 Aprendizajes Técnicos

Este proyecto demuestra dominio de los siguientes conocimientos:

| Área | Conocimiento Demostrado |
|------|------------------------|
| **Arquitectura de Software** | Implementación de Clean Architecture con cuatro proyectos independientes en una solution .NET |
| **C# / .NET 8** | Uso de nullable reference types, record-like properties, `ICollection<T>` para navegación de relaciones, partial classes para EF Core |
| **Entity Framework Core** | Enfoque Database-First con scaffolding, configuración Fluent API en `OnModelCreating`, uso de `FromSqlRaw` y `ExecuteSqlRaw` para stored procedures |
| **SQL Server** | Diseño de base de datos relacional con FK, índices únicos compuestos, valores por defecto y stored procedures |
| **ASP.NET Core MVC** | Pipeline HTTP, routing convencional, Razor Views con secciones (`@section Styles`, `@section Scripts`), layout compartido |
| **Dependency Injection** | Registro y resolución de dependencias con el contenedor nativo de .NET |
| **Design Patterns** | Repository Pattern, DTO Pattern, Dependency Injection Pattern |
| **Frontend** | CSS personalizado con variables, diseño de landing page moderno, uso de Bootstrap para layout responsivo |
| **Buenas Prácticas** | Separación de concerns, interfaces como contratos, nomenclatura consistente |

---

## 👨‍💻 Autor

<br>

### Félix Alvarado

**Junior Backend Developer · Systems Engineering Student**

> Apasionado por la arquitectura de software, los sistemas backend robustos y el desarrollo de soluciones con impacto real. Actualmente cursando el 7mo semestre de Ingeniería en Sistemas en la Universidad San Isidro Labrador (UISIL), Costa Rica.

<br>

**Stack Principal:**

![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF_Core-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Java](https://img.shields.io/badge/Java-ED8B00?style=flat-square&logo=openjdk&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=flat-square&logo=python&logoColor=white)

**Encuéntrame:**

[![GitHub](https://img.shields.io/badge/GitHub-Felix--alvx-181717?style=flat-square&logo=github)](https://github.com/Felix-alvx)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Félix_Alvarado-0A66C2?style=flat-square&logo=linkedin)](https://linkedin.com/in/félix-alvarado-84a3852b4/)



---

<div align="center">

**⭐ Si este proyecto te parece interesante, deja una estrella en GitHub.**


</div>
