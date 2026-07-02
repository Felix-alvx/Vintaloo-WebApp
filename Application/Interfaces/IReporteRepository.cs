using Domain.Data; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReporteRepository
    {
        // Data access to database  
        public List<Reporte> ObtenerReportes();
        public void CrearReporte(Reporte reporte); 


    }
}
