using Application.DTOs;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFavoritoService
    {
        List<ArticuloDTO> ObtenerFavoritosUsuario(int idUsuario);
        void AgregarFavorito(int idUsuario, int idArticulo);
        void EliminarFavorito(int idUsuario, int idArticulo);
    }
}
