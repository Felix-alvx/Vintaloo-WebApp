using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        
   
        private readonly IArticuloService _service;
        
         

        public DashboardController(IArticuloService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
           var articulos = _service.ObtenerTodosArticulos();
            return View(articulos);
        }
    }
}

