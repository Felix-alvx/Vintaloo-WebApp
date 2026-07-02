using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Servicies
{
    public class ImagenArticuloService : IImagenArticuloService
    {
        private readonly IImagenArticuloRepository _repo;

        public ImagenArticuloService(IImagenArticuloRepository repo)
        {
            _repo= repo;
        }
        // Metodo para obtener Imagenes del Articulo

        public List<ImagenesArticulo> ObtenerPorArticulo(int idArticulo)
        {
            if (idArticulo <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.", nameof(idArticulo));

            return _repo.ObtenerPorArticulo(idArticulo);
        }

        // Merodo para guardar imageness

        public void GuardarImagen(ImagenesArticulo imagen)
        {
            if (imagen == null)
                throw new ArgumentNullException(nameof(imagen), "La imagen no puede ser nula.");

            if (imagen.IdArticulo <= 0)
                throw new ArgumentException("El ID del artículo asociado debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(imagen.UrlImagen))
                throw new ArgumentException("La URL de la imagen es obligatoria.");

            _repo.Guardar(imagen);
        }
    }
}
