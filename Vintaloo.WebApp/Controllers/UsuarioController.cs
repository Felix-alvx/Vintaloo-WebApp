using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{

    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _service;
        private readonly IArticuloService _serviceArticulo;

    
        public UsuarioController(IUsuarioService service, IArticuloService serviceArticulo)
        {
            _service = service;
            _serviceArticulo = serviceArticulo;
        }

        /// <summary>
        /// Muestra la lista de todos los usuarios registrados.
        /// </summary>
        [HttpGet]
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
        public IActionResult Crear()
        {
            return View(new Usuario());
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario.
        /// </summary>
        /// <param name="usuario">Entidad usuario a registrar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            _service.GuardarUsuarioService(usuario);
            TempData["Mensaje"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra el formulario para editar un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuario = _service.ObtenerUsuarioPorId(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        /// <summary>
        /// Procesa la actualización de un usuario.
        /// </summary>
        /// <param name="usuario">Datos actualizados del usuario.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            _service.ActualizarUsuario(usuario);
            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra la vista de confirmación para eliminar un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        [HttpGet]
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
            if (!ModelState.IsValid)
                return View(dto);

            var usuario = new Usuario
            {
                IdUsuario = dto.IdUsuario,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Ubicacion = dto.Ubicacion,
                Estado = "activo"
            };

            // Procesamiento de imagen de perfil
            if (fotoPerfil != null && fotoPerfil.Length > 0)
            {
                Directory.CreateDirectory(Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "perfiles"));

                var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(fotoPerfil.FileName)}";
                var ruta = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "perfiles", nombreArchivo);

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

            return int.Parse(claim.Value);
        }
    }
}