using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{

    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _service;
        private readonly IArticuloService _serviceArticulo;
        private readonly IWebHostEnvironment _env;

    
        public UsuarioController(
            IUsuarioService service,
            IArticuloService serviceArticulo,
            IWebHostEnvironment env)
        {
            _service = service;
            _serviceArticulo = serviceArticulo;
            _env = env;
        }

        /// <summary>
        /// Muestra la lista de todos los usuarios registrados.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Index()
        {
            var usuarios = _service.ObtenerUsuarios();
            return View(usuarios);
        }

        /// <summary>
        /// Muestra el detalle de un usuario específico.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Detalle(int id)
        {
            var usuario = _service.ObtenerUsuarioPorId(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo usuario.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Crear()
        {
            return View(new CreateUsuarioDTO());
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario.
        /// </summary>
        /// <param name="dto">Datos del usuario a registrar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public IActionResult Crear(CreateUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (!_service.CrearUsuario(dto))
            {
                ModelState.AddModelError(nameof(dto.Correo), "El correo ya está registrado.");
                return View(dto);
            }

            TempData["Mensaje"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra el formulario para editar un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Editar(int id)
        {
            var usuario = _service.ObtenerUsuarioPorId(id);
            if (usuario == null)
                return NotFound();

            var dto = new UpdateUsuarioDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Telefono = usuario.Telefono,
                Ubicacion = usuario.Ubicacion,
                Estado = usuario.Estado ?? "activo",
                IdRol = usuario.Rol == "admin" ? 1 : 2
            };

            return View(dto);
        }

        /// <summary>
        /// Procesa la actualización de un usuario.
        /// </summary>
        /// <param name="dto">Datos actualizados del usuario.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public IActionResult Editar(UpdateUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            _service.ActualizarUsuarioAdmin(dto);
            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra la vista de confirmación para eliminar un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Eliminar(int id)
        {
            var usuario = _service.ObtenerUsuarioPorId(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación de un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public IActionResult EliminarConfirmado(int id)
        {
            _service.EliminarUsuario(id);
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- PERFIL DE USUARIO ----------------

        /// <summary>
        /// Muestra el perfil del usuario autenticado junto con sus artículos publicados.
        /// </summary>
        [HttpGet]
        public IActionResult Perfil()
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            var usuario = _service.ObtenerUsuarioPorId(idUsuario);
            if (usuario == null)
                return NotFound();

            var articulos = _serviceArticulo.ObtenerPorUsuario(idUsuario);

            var perfil = new PerfilDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                Ubicacion = usuario.Ubicacion,
                FotoPerfil = usuario.FotoPerfil,
                Articulos = articulos
            };

            return View(perfil);
        }

        /// <summary>
        /// Muestra el formulario para editar el perfil del usuario autenticado.
        /// </summary>
        [HttpGet]
        public IActionResult EditarPerfil()
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            var usuario = _service.ObtenerUsuarioPorId(idUsuario);
            if (usuario == null)
                return NotFound();

            var dto = new EditarPerfilDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Telefono = usuario.Telefono,
                Ubicacion = usuario.Ubicacion
            };

            ViewBag.FotoActual = usuario.FotoPerfil;

            return View(dto);
        }

        /// <summary>
        /// Procesa la actualización del perfil del usuario autenticado,
        /// incluyendo la carga de una nueva imagen de perfil.
        /// </summary>
        /// <param name="dto">Datos editables del perfil.</param>
        /// <param name="fotoPerfil">Archivo de imagen opcional.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(EditarPerfilDTO dto, IFormFile? fotoPerfil)
        {
            // El ID del perfil siempre proviene del token, nunca del formulario.
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            var usuarioActual = _service.ObtenerUsuarioPorId(idUsuario);
            if (usuarioActual == null)
                return NotFound();

            ViewBag.FotoActual = usuarioActual.FotoPerfil;

            if (!ModelState.IsValid)
                return View(dto);

            var usuario = new Usuario
            {
                IdUsuario = idUsuario,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Ubicacion = dto.Ubicacion
            };

            // Procesamiento de imagen de perfil
            if (fotoPerfil != null && fotoPerfil.Length > 0)
            {
                var extension = Path.GetExtension(fotoPerfil.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                const long maxBytes = 2 * 1024 * 1024;

                if (!extensionesPermitidas.Contains(extension) || fotoPerfil.Length > maxBytes)
                {
                    ModelState.AddModelError(
                        "fotoPerfil",
                        "La foto debe ser JPG, PNG o WEBP y no superar 2 MB.");
                    return View(dto);
                }

                var webRoot = _env.WebRootPath
                    ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var carpeta = Path.Combine(webRoot, "perfiles");
                Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(ruta, FileMode.Create);
                fotoPerfil.CopyTo(stream);

                usuario.FotoPerfil = $"/perfiles/{nombreArchivo}";
            }

            _service.ActualizarUsuario(usuario);
            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        /// <summary>
        /// Permite cambiar el estado de un artículo desde el perfil del usuario.
        /// </summary>
        /// <param name="idArticulo">ID del artículo.</param>
        /// <param name="estado">Nuevo estado.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstadoArticulo(int idArticulo, string estado)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(estado))
            {
                TempData["Error"] = "El estado no puede estar vacío.";
                return RedirectToAction(nameof(Perfil));
            }

            var articulo = _serviceArticulo.ObtenerPorId(idArticulo);
            if (articulo == null)
                return NotFound();

            if (articulo.IdUsuario != idUsuario)
                return Forbid();

            _serviceArticulo.CambiarEstado(idArticulo, estado);
            TempData["Mensaje"] = "Estado actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        /// <summary>
        /// Obtiene el ID del usuario autenticado desde los Claims.
        /// </summary>
        /// <returns>ID del usuario o 0 si no está autenticado.</returns>
        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null) return 0;

            return int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}