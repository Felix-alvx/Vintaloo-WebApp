using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
  
    /// Controlador encargado de gestionar las operaciones relacionadas con los artículos.
    /// Permite listar, crear, visualizar, actualizar estado y eliminar artículos.
   
    public class ArticuloController : Controller
    {
        private readonly IArticuloService _service;
        private readonly ICategoriaService _categoriaService;

      
        public ArticuloController(IArticuloService service, ICategoriaService categoriaService)
        {
            _service = service;
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Muestra la lista de todos los artículos disponibles.
        /// </summary>
        /// <returns>Vista con la lista de artículos.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var articulos = _service.ObtenerTodosArticulos();
            return View(articulos);
        }

        /// <summary>
        /// Muestra el detalle de un artículo específico.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <returns>Vista con el detalle del artículo o NotFound si no existe.</returns>
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var articulo = _service.ObtenerPorId(id);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo artículo.
        /// </summary>
        /// <returns>Vista de creación de artículo.</returns>
        [HttpGet]
        public IActionResult Crear()
        {
            // Se cargan las categorías para el formulario
            ViewBag.Categorias = _categoriaService.ObtenerTodas();
            return View(new CreateArticuloDTO());
        }

        /// <summary>
        /// Procesa la creación de un nuevo artículo.
        /// </summary>
        /// <param name="dto">Datos del artículo a crear.</param>
        /// <param name="imagenes">Lista de imágenes subidas.</param>
        /// <returns>Redirige al dashboard si se crea correctamente.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CreateArticuloDTO dto, List<IFormFile> imagenes)
        {
            ViewBag.Categorias = _categoriaService.ObtenerTodas();

            if (!ModelState.IsValid)
                return View(dto);

            // Se obtiene el ID del usuario autenticado
            var idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            // Procesamiento y almacenamiento de imágenes en el servidor
            foreach (var archivo in imagenes)
            {
                if (archivo.Length > 0)
                {
                    var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
                    var ruta = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "imagen", nombreArchivo
                    );

                    using var stream = new FileStream(ruta, FileMode.Create);
                    archivo.CopyTo(stream);

                    // Se guarda la ruta relativa en el DTO
                    dto.Imagenes.Add($"/imagen/{nombreArchivo}");
                }
            }

            // Se crea el artículo asociado al usuario autenticado
            _service.CrearArticulos(dto, idUsuario);

            TempData["Mensaje"] = "Artículo publicado correctamente.";
            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Muestra el formulario para cambiar el estado de un artículo.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <returns>Vista con los datos del artículo.</returns>
        [HttpGet]
        public IActionResult CambiarEstado(int id)
        {
            var articulo = _service.ObtenerPorId(id);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        /// <summary>
        /// Procesa el cambio de estado de un artículo.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <param name="estado">Nuevo estado del artículo.</param>
        /// <returns>Redirige a la lista de artículos.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                TempData["Error"] = "El estado no puede estar vacío.";
                return RedirectToAction(nameof(CambiarEstado), new { id });
            }

            _service.CambiarEstado(id, estado);
            TempData["Mensaje"] = "Estado actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra la vista de confirmación para eliminar un artículo.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <returns>Vista de confirmación o NotFound si no existe.</returns>
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var articulo = _service.ObtenerPorId(id);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        /// <summary>
        /// Confirma y ejecuta la eliminación de un artículo.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <returns>Redirige a la lista de artículos.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            _service.EliminarArticulo(id);
            TempData["Mensaje"] = "Artículo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
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