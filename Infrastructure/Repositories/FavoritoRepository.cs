using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Application.Interfaces;
using Domain.Data;


namespace Vintaloo_WebApp.src.Infrastructure.Repositories
{
    public class FavoritoRepository : IFavoritoRepository
    {
        private readonly VintalooDbContext _context;

        public FavoritoRepository(VintalooDbContext context)
        {
            _context = context;
        }

        // Obtener favoritos de un usuario
        public List<Articulo> ObtenerFavoritosUsuario(int idUsuario)
        {
            return _context.Favoritos
                .Where(f => f.IdUsuario == idUsuario)
                .Include(f => f.IdArticuloNavigation)
                    .ThenInclude(a => a.ImagenesArticulos)
                .Select(f => f.IdArticuloNavigation)
                .ToList();
        }

        // Agregar favorito
        public void AgregarFavorito(int idUsuario, int idArticulo)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_agregar_fav @id_usuario={0}, @id_articulo={1}",
                idUsuario,
                idArticulo
            );
        }

        // Eliminar favorito
        public void EliminarFavorito(int idUsuario, int idArticulo)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_eliminar_favorito @id_usuario={0}, @id_articulo={1}",
                idUsuario,
                idArticulo
            );
        }
    }
}

