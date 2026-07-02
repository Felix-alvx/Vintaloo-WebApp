using Domain.Data; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFavoritoRepository
    {
        //Métodos para Favoritos 
        public List<Articulo> ObtenerFavoritosUsuario(int idUsuario);
        public void AgregarFavorito(int idUsuario, int idArticulo);
        public void EliminarFavorito(int idUsuario, int idArticulo); 
    }
}
