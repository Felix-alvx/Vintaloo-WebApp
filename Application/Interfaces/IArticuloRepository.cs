using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IArticuloRepository
    {
        // Métodos BD de Articulo 

      
        public List<Articulo> obtenerArticulos();

        public Articulo? obtenerArticuloPorId(int id);

        public int GuardarArticulos(Articulo articulo);  // Ahora debería retornar el ID

        public void ActualizarEstado(int idArticulo, string estado);


        public void Eliminar(int id);

        public List<Articulo> ObtenerPorUsuario(int idUsuario);
    }
}
