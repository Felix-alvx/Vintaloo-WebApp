using System;
using System.Collections.Generic;
using Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class VintalooDbContext : DbContext
{
    public VintalooDbContext()
    {
    }

    public VintalooDbContext(DbContextOptions<VintalooDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Articulo> Articulos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<ImagenesArticulo> ImagenesArticulos { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    // Entidad creada para que reciva el ID 
    public DbSet<IdResult> IdResults { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=FGABRIEL14\\SQLEXPRESS;Database=vintaloo_db;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Articulo>(entity =>
        {
            entity.HasKey(e => e.IdArticulo).HasName("PK__articulo__3F6E82880360A385");

            entity.Property(e => e.EstadoPublicacion).HasDefaultValue("disponible");
            entity.Property(e => e.FechaPublicacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Articulos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__articulos__id_ca__571DF1D5");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Articulos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__articulos__id_us__5629CD9C");
        });
        

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__categori__CD54BC5A8AE21DAA");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PK__favorito__78F875AE178EEBF9");

            entity.Property(e => e.FechaAgregado).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdArticuloNavigation).WithMany(p => p.Favoritos).HasConstraintName("FK__favoritos__id_ar__5FB337D6");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos).HasConstraintName("FK__favoritos__id_us__5EBF139D");
        });

        modelBuilder.Entity<ImagenesArticulo>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__imagenes__27CC26896C89CDED");

            entity.HasOne(d => d.IdArticuloNavigation).WithMany(p => p.ImagenesArticulos).HasConstraintName("FK__imagenes___id_ar__59FA5E80");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.IdReporte).HasName("PK__reportes__87E4F5CBBD48DA17");

            entity.Property(e => e.Estado).HasDefaultValue("pendiente");
            entity.Property(e => e.FechaReporte).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdArticuloNavigation).WithMany(p => p.Reportes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reportes__id_art__66603565");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reportes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reportes__id_usu__656C112C");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuarios__4E3E04ADE6F27E8B");

            entity.Property(e => e.Estado).HasDefaultValue("activo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
        });

        // Model de la Entidad creada para que reciba el ID
        modelBuilder.Entity<IdResult>().HasNoKey();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
