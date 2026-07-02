using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IArticuloService
    {
        public List<ArticuloDTO> ObtenerTodosArticulos();
        public ArticuloDTO? ObtenerPorId(int id);
        public void CrearArticulos(CreateArticuloDTO dto, int idUsuario);
        public void CambiarEstado(int idArticulo, string estado);
        public void EliminarArticulo(int id);
        public List<ArticuloDTO> ObtenerPorUsuario(int idUsuario); 
    }
}
