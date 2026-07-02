using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _service;

        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        // GET /Categoria/Index
        [HttpGet]
        public IActionResult Index()
        {
            var categorias = _service.ObtenerTodas();
            return View(categorias);
        }

        // GET /Categoria/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CreateCategoriaDTO());
        }

        // POST /Categoria/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CreateCategoriaDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _service.GuardarCategoria(categoria);
            TempData["Mensaje"] = "Categoría creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Categoria/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var categorias = _service.ObtenerTodas();
            var categoria = categorias.FirstOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
                return NotFound();

            var dto = new UpdateCategoriaDTO
            {
                IdCategoria = categoria.IdCategoria,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return View(dto);
        }

        // POST /Categoria/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(UpdateCategoriaDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var categoria = new Categoria
            {
                IdCategoria = dto.IdCategoria,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _service.ActualizarCategoria(categoria);
            TempData["Mensaje"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Categoria/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var categorias = _service.ObtenerTodas();
            var categoria = categorias.FirstOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        // POST /Categoria/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            _service.EliminarCategoria(id);
            TempData["Mensaje"] = "Categoría eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
