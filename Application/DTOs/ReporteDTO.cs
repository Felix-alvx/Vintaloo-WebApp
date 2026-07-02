using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ReporteDTO
    {
        public int IdReporte { get; set; }
        public int IdUsuario { get; set; }
        public int IdArticulo { get; set; }
        public string Motivo { get; set; } = null!;
    }
}
