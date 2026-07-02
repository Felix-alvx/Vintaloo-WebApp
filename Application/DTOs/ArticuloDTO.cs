using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ArticuloDTO
    {
        // DTO para articulo
        // ── Datos básicos ──
        public int IdArticulo { get; set; }
        public string Titulo { get; set; } = null!;
        public decimal Precio { get; set; }
        public string EstadoProducto { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? EstadoPublicacion { get; set; }
        public string? Ubicacion { get; set; }

        // ── Imagen principal (para el Dashboard) ──
        public string? UrlImagen { get; set; }

        // ── Galería completa (para la vista de Detalle) ──
        public List<ImagenDTO>? ImagenesArticulos { get; set; }

        // ── Datos del vendedor (para la vista de Detalle) ──
        public string? VendedorNombre { get; set; }
        public string? VendedorFoto { get; set; }
        public decimal? VendedorRating { get; set; }
        public int? VendedorVentas { get; set; }
    }

    public class ImagenDTO
    {
        public int IdImagen { get; set; }
        public string UrlImagen { get; set; } = null!;
    }
}

