using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IImagenArticuloService
    {
        public List<ImagenesArticulo> ObtenerPorArticulo(int idArticulo);
        public void GuardarImagen(ImagenesArticulo imagen);

    }
}
