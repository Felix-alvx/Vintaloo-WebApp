using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Servicies
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _repo;

        public ReporteService(IReporteRepository repo)
        {
            _repo = repo;
        }

        // Metodo para obtner los reportes 
        public List<Reporte> ObtenerReportes()
        {
            return _repo.ObtenerReportes();
        }

        // Metodo para crear Reportes
        public void CrearReporte(Reporte reporte)
        {
            if (reporte == null)
                throw new ArgumentNullException(nameof(reporte), "El reporte no puede ser nulo.");

            if (reporte.IdUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.");

            if (reporte.IdArticulo <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(reporte.Motivo))
                throw new ArgumentException("El motivo del reporte es obligatorio.");

            _repo.CrearReporte(reporte);
        }
    }
}
