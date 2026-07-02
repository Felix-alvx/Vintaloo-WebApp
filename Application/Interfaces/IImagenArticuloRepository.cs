using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IImagenArticuloRepository
    {
        // Data acces to databases
        public List<ImagenesArticulo> ObtenerPorArticulo(int idArticulo);

        public void Guardar(ImagenesArticulo imagen); 
    }
}
