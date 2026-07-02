using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Data
{

    [Table("articulos")]
    public partial class Articulo
    {
        [Key]
        [Column("id_articulo")]
        public int IdArticulo { get; set; } 

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("titulo")]
        [StringLength(150)]
        [Unicode(false)]
        public string Titulo { get; set; } = null!;

        [Column("descripcion")]
        [Unicode(false)]
        public string Descripcion { get; set; } = null!;

        [Column("precio", TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [Column("estado_producto")]
        [StringLength(20)]
        [Unicode(false)]
        public string EstadoProducto { get; set; } = null!;

        [Column("estado_publicacion")]
        [StringLength(20)]
        [Unicode(false)]
        public string? EstadoPublicacion { get; set; }

        [Column("ubicacion")]
        [StringLength(150)]
        [Unicode(false)]
        public string? Ubicacion { get; set; }

        [Column("fecha_publicacion", TypeName = "datetime")]


        public DateTime? FechaPublicacion { get; set; }

        [InverseProperty("IdArticuloNavigation")]
        public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

        [ForeignKey("IdCategoria")]
        [InverseProperty("Articulos")]
        public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        [InverseProperty("Articulos")]
        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

        [InverseProperty("IdArticuloNavigation")]
        public virtual ICollection<ImagenesArticulo> ImagenesArticulos { get; set; } = new List<ImagenesArticulo>();

        [InverseProperty("IdArticuloNavigation")]
        public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
    }

}


