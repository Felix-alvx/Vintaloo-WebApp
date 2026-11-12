using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    [Authorize]
    public class ReporteController : Controller
    {
        private readonly IReporteService _service;

        public ReporteController(IReporteService service)
        {
            _service = service;
        }

        // GET /Reporte/Index
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Index()
        {
            var reportes = _service.ObtenerReportes();
            return View(reportes);
        }

        // GET /Reporte/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Reporte());
        }

        // POST /Reporte/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Reporte reporte)
        {
            if (reporte.IdArticulo <= 0)
                ModelState.AddModelError(nameof(reporte.IdArticulo), "Indica un artículo válido.");

            if (string.IsNullOrWhiteSpace(reporte.Motivo))
                ModelState.AddModelError(nameof(reporte.Motivo), "El motivo es obligatorio.");
            else if (reporte.Motivo.Length > 255)
                ModelState.AddModelError(nameof(reporte.Motivo), "El motivo no puede superar 255 caracteres.");

            if (!ModelState.IsValid)
                return View(reporte);

            // El reportante siempre es el usuario autenticado.
            reporte.IdUsuario = ObtenerIdUsuarioActual();
            if (reporte.IdUsuario == 0)
                return RedirectToAction("Login", "Auth");

            _service.CrearReporte(reporte);
            TempData["Mensaje"] = "Reporte enviado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null) return 0;

            return int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}