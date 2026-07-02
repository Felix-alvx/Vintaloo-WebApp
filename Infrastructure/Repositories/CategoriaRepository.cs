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
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly VintalooDbContext _context;

        public CategoriaRepository(VintalooDbContext context)
        {
            _context = context;
        }

        public List<Categoria> ObtenerTodas()
        {
            return _context.Categorias
                .FromSqlRaw("EXEC sp_obtener_categoria")
                .ToList();
        }

        public void Guardar(Categoria categoria)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_insertar_categoria @nombre={0}, @descripcion={1}",
                categoria.Nombre,
                categoria.Descripcion
            );
        }

        public void Actualizar(Categoria categoria)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_actualizar_categoria @id_categoria={0}, @nombre={1}, @descripcion={2}",
                categoria.IdCategoria,
                categoria.Nombre,
                categoria.Descripcion
            );
        }

        public void Eliminar(int id)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_eliminar_categoria @id_categoria={0}",
                id
            );
        }
    }
}
