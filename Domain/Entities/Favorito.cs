using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Data;

[Table("favoritos")]
[Index("IdUsuario", "IdArticulo", Name = "UQ_favoritos", IsUnique = true)]
public partial class Favorito
{
    [Key]
    [Column("id_favorito")]
    public int IdFavorito { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("id_articulo")]
    public int IdArticulo { get; set; }

    [Column("fecha_agregado", TypeName = "datetime")]
    public DateTime? FechaAgregado { get; set; }

    [ForeignKey("IdArticulo")]
    [InverseProperty("Favoritos")]
    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Favoritos")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
