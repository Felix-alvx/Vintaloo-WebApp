using Infrastructure.Data;
using Application.Interfaces; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Data;


namespace Infrastructure.Repositories
{
    public class ImagenArticuloRepository : IImagenArticuloRepository
    {
        private readonly VintalooDbContext _context;

        public ImagenArticuloRepository(VintalooDbContext context)
        {
            _context = context;
        }

        // Obtener imágenes de un artículo
        public List<ImagenesArticulo> ObtenerPorArticulo(int idArticulo)
        {
            return _context.ImagenesArticulos
                .FromSqlRaw("EXEC sp_obtener_imagenes_articulo @id_articulo={0}", idArticulo)
                .ToList();
        }

        // Insertar imagen
        public void Guardar(ImagenesArticulo imagen)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_insertar_imagen_articulo @id_articulo={0}, @url_imagen={1}",
                imagen.IdArticulo,
                imagen.UrlImagen
            );
        }
    }
}
