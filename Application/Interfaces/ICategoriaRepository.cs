using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Application.Interfaces
{
    public interface ICategoriaRepository
    {

        // Métodos BD para Categoria 

        public List<Categoria> ObtenerTodas();

        public void Guardar(Categoria categoria);

        public void Actualizar(Categoria categoria);

        public void Eliminar(int id); 
    }
}
