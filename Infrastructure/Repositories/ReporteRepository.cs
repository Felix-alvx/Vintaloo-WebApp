using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Data;


namespace Infrastructure.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly VintalooDbContext _context;

        public ReporteRepository(VintalooDbContext context)
        {
            _context = context;
        }


        // Obtener reportes
        public List<Reporte> ObtenerReportes()
        {
            return _context.Reportes
                .FromSqlRaw("EXEC sp_obtener_reportes")
                .ToList();
        }

        // Crear reporte
        public void CrearReporte(Reporte reporte)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_crear_reporte @id_usuario={0}, @id_articulo={1}, @motivo={2}, @descripcion={3}",
                reporte.IdUsuario,
                reporte.IdArticulo,
                reporte.Motivo,
                reporte.Descripcion
            );
        }
    }
}
