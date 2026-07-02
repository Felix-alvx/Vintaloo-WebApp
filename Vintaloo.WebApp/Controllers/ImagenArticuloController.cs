using Application.Interfaces;
using Domain.Data;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/imagenes")]
    public class ImagenArticuloController : ControllerBase
    {
        private readonly IImagenArticuloService _service;

        public ImagenArticuloController(IImagenArticuloService service)
        {
            _service = service;
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
        public IActionResult Post([FromBody] ImagenesArticulo imagen)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _service.GuardarImagen(imagen);
            return Ok(new { mensaje = "Imagen guardada correctamente." });
        }
    }
}