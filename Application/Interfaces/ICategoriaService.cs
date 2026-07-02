using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoriaService
    {
        public List<Categoria> ObtenerTodas();
        public void GuardarCategoria(Categoria categoria);
        public void ActualizarCategoria(Categoria categoria);
        public void EliminarCategoria(int id); 

    }
}
