using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class ReporteController : Controller
    {
        private readonly IReporteService _service;

        public ReporteController(IReporteService service)
        {
            _service = service;
        }

        // GET /Reporte/Index
        [HttpGet]
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
            if (!ModelState.IsValid)
                return View(reporte);

            _service.CrearReporte(reporte);
            TempData["Mensaje"] = "Reporte enviado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}