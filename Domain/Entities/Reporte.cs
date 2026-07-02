using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Data;

[Table("reportes")]
public partial class Reporte
{
    [Key]
    [Column("id_reporte")]
    public int IdReporte { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_articulo")]
    public int IdArticulo { get; set; }

    [Column("motivo")]
    [StringLength(255)]
    [Unicode(false)]
    public string Motivo { get; set; } = null!;

    [Column("descripcion")]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [Column("fecha_reporte", TypeName = "datetime")]
    public DateTime? FechaReporte { get; set; }

    [Column("estado")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [ForeignKey("IdArticulo")]
    [InverseProperty("Reportes")]
    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Reportes")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
