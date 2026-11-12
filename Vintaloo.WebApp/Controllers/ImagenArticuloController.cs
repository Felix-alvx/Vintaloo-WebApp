using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/imagenes")]
    public class ImagenArticuloController : ControllerBase
    {
        private readonly IImagenArticuloService _service;
        private readonly IArticuloService _articuloService;

        public ImagenArticuloController(
            IImagenArticuloService service,
            IArticuloService articuloService)
        {
            _service = service;
            _articuloService = articuloService;
        }

        // GET api/imagenes/{idArticulo}
        [HttpGet("{idArticulo}")]
        public IActionResult Get(int idArticulo)
        {
            var imagenes = _service.ObtenerPorArticulo(idArticulo);
            return Ok(imagenes);
        }

        // POST api/imagenes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] ImagenesArticulo imagen)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var idUsuario = ObtenerIdUsuarioActual();
            if (idUsuario == 0)
                return Unauthorized();

            var articulo = _articuloService.ObtenerPorId(imagen.IdArticulo);
            if (articulo == null)
                return NotFound();

            if (articulo.IdUsuario != idUsuario)
                return Forbid();

            _service.GuardarImagen(imagen);
            return Ok(new { mensaje = "Imagen guardada correctamente." });
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}