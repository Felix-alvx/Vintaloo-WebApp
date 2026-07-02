using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class FavoritoController : Controller
    {
        private readonly IFavoritoService _service;

        public FavoritoController(IFavoritoService service)
        {
            _service = service;
        }

        // GET /Favorito
        [HttpGet]
        public IActionResult Index()
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            var favoritos = _service.ObtenerFavoritosUsuario(idUsuario);
            return View("Agregar", favoritos);
        }

        // POST /Favorito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int idArticulo)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            _service.AgregarFavorito(idUsuario, idArticulo);
            TempData["Mensaje"] = "Agregado a favoritos.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Favorito/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int idArticulo)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return RedirectToAction("Login", "Auth");

            _service.EliminarFavorito(idUsuario, idArticulo);
            TempData["Mensaje"] = "Eliminado de favoritos.";
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null) return 0;
            return int.Parse(claim.Value);
        }
    }
}
