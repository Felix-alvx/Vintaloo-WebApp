using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Data;

[Table("usuarios")]
[Index("Correo", Name = "UQ__usuarios__2A586E0BE35A39B8", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [Column("correo")]
    [StringLength(150)]
    [Unicode(false)]
    public string Correo { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [Column("telefono")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Telefono { get; set; }

    [Column("ubicacion")]
    [StringLength(150)]
    [Unicode(false)]
    public string? Ubicacion { get; set; }

    [Column("foto_perfil")]
    [StringLength(255)]
    [Unicode(false)]
    public string? FotoPerfil { get; set; }

    [Column("fecha_registro", TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [Column("estado")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}
