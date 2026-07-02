using Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Data;
using Microsoft.Data.SqlClient;



namespace Infrastructure.Repositories
{
    public class ArticuloRepository : IArticuloRepository
    {

        private readonly VintalooDbContext _context; 

        public ArticuloRepository(VintalooDbContext context) 
        { 
            _context = context;
        }


        // Método para obtener los Articulos
        public List<Articulo> obtenerArticulos()
        {
            return _context.Articulos
            .Include(a => a.ImagenesArticulos) // ← carga las imágenes
            .ToList();
        }



        // Método para obtner articulos por ID 
        public Articulo? obtenerArticuloPorId(int id)
        {

            return _context.Articulos
                    .Include(a => a.ImagenesArticulos)       // carga las imágenes
                    .Include(a => a.IdUsuarioNavigation)      // carga los datos del vendedor
                    .FirstOrDefault(a => a.IdArticulo == id);
        }


        // Método de Insertar Articulos
        public int GuardarArticulos(Articulo articulo)
        {
            // Usamos FromSqlRaw para capturar el SELECT SCOPE_IDENTITY() que retorna el SP
            var resultado = _context.Set<IdResult>()
                .FromSqlRaw(
                    "EXEC sp_insertar_articulo @id_usuario={0}, @id_categoria={1}, @titulo={2}, @descripcion={3}, @precio={4}, @estado_producto={5}, @ubicacion={6}",
                    articulo.IdUsuario,
                    articulo.IdCategoria,
                    articulo.Titulo,
                    articulo.Descripcion,
                    articulo.Precio,
                    articulo.EstadoProducto,
                    articulo.Ubicacion
                ).AsEnumerable().FirstOrDefault();

            return resultado?.Id ?? 0;
        }

        // Método de Actualizar Estado del Articulo 
        public void ActualizarEstado(int idArticulo, string estado)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_cambiar_estado_articulo @id_articulo={0}, @estado_publicacion={1}",
                idArticulo,
                estado
            );
        }

        // Método de Eliminar
        public void Eliminar(int id)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_eliminar_articulo @id_articulo={0}",
                id
            );
        }

        // METODO PARA OBTENER Usuario.id actual
        // Se utiliza en la vista Perfil 
        public List<Articulo> ObtenerPorUsuario(int idUsuario)
        {
            return _context.Articulos
                .Include(a => a.ImagenesArticulos)
                .Where(a => a.IdUsuario == idUsuario)
                .OrderByDescending(a => a.FechaPublicacion)
                .ToList();
        }
    }
}
