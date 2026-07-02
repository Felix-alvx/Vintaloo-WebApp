using Microsoft.EntityFrameworkCore; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Data;

[Table("imagenes_articulos")]
public partial class ImagenesArticulo
{
    [Key]
    [Column("id_imagen")]
    public int IdImagen { get; set; }

    [Column("id_articulo")]
    public int IdArticulo { get; set; }

    [Column("url_imagen")]
    [StringLength(255)]
    [Unicode(false)]
    public string UrlImagen { get; set; } = null!;

    [ForeignKey("IdArticulo")]
    [InverseProperty("ImagenesArticulos")]
    public virtual Articulo IdArticuloNavigation { get; set; } = null!;
}
