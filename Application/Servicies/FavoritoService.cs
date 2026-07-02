using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Servicies
{
    public class FavoritoService : IFavoritoService
    {
        private readonly IFavoritoRepository _repo;

        public FavoritoService(IFavoritoRepository repo)
        {
            _repo = repo;
        }

        // Metodo para obtener todos los favoritos del Usuario 
        public List<ArticuloDTO> ObtenerFavoritosUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(idUsuario));

            return _repo.ObtenerFavoritosUsuario(idUsuario)
                .Select(a => new ArticuloDTO
                {
                    IdArticulo = a.IdArticulo,
                    Titulo = a.Titulo,
                    Precio = a.Precio,
                    EstadoProducto = a.EstadoProducto,
                    EstadoPublicacion = a.EstadoPublicacion,
                    Descripcion = a.Descripcion,
                    Ubicacion = a.Ubicacion,
                    UrlImagen = a.ImagenesArticulos
                                         .Select(i => i.UrlImagen)
                                         .FirstOrDefault()
                }).ToList();
        }

        // Metodo para Agregar un Fav del Usuario
        public void AgregarFavorito(int idUsuario, int idArticulo)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(idUsuario));

            if (idArticulo <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.", nameof(idArticulo));

            _repo.AgregarFavorito(idUsuario, idArticulo);
        }

        //Metodo para Eliminar un Favorito
        public void EliminarFavorito(int idUsuario, int idArticulo)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(idUsuario));

            if (idArticulo <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.", nameof(idArticulo));

            _repo.EliminarFavorito(idUsuario, idArticulo);
        }
    }
}
