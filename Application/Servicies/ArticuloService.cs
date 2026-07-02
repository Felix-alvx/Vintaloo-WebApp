using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Servicies
{

    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _repo;
        private readonly IImagenArticuloRepository _imagenRepo;

        public ArticuloService(IArticuloRepository repo, IImagenArticuloRepository imagenRepo)
        {
            _repo = repo;
            _imagenRepo = imagenRepo;
        }

        /// <summary>
        /// Obtiene todos los artículos disponibles en el sistema.
        /// </summary>
        /// <returns>Lista de artículos en formato DTO.</returns>
        public List<ArticuloDTO> ObtenerTodosArticulos()
        {
            return _repo.obtenerArticulos()
              .Select(a => new ArticuloDTO
              {
                  IdArticulo = a.IdArticulo,
                  Titulo = a.Titulo,
                  Precio = a.Precio,
                  EstadoProducto = a.EstadoProducto,
                  Descripcion = a.Descripcion,
                  EstadoPublicacion = a.EstadoPublicacion,

                  // Se obtiene la primera imagen asociada al artículo (preview)
                  UrlImagen = a.ImagenesArticulos
                                   .Select(i => i.UrlImagen)
                                   .FirstOrDefault()
              }).ToList();
        }


        /// <summary>
        /// Obtiene un artículo específico por su identificador.
        /// </summary>
        /// <param name="id">ID del artículo.</param>
        /// <returns>Artículo en formato DTO o null si no existe.</returns>
        /// <exception cref="ArgumentException">Se lanza si el ID es inválido.</exception>
        public ArticuloDTO? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.", nameof(id));

            var a = _repo.obtenerArticuloPorId(id);
            if (a == null) return null;

            return new ArticuloDTO
            {
                IdArticulo = a.IdArticulo,
                Titulo = a.Titulo,
                Precio = a.Precio,
                EstadoProducto = a.EstadoProducto,
                Descripcion = a.Descripcion,
                EstadoPublicacion = a.EstadoPublicacion,
                Ubicacion = a.Ubicacion,

                // Imagen principal (vista previa)
                UrlImagen = a.ImagenesArticulos
                             .Select(i => i.UrlImagen)
                             .FirstOrDefault(),

                // Lista completa de imágenes (galería)
                ImagenesArticulos = a.ImagenesArticulos
                                     .Select(i => new ImagenDTO
                                     {
                                         IdImagen = i.IdImagen,
                                         UrlImagen = i.UrlImagen
                                     }).ToList(),

                // Información del vendedor
                VendedorNombre = a.IdUsuarioNavigation?.Nombre,
                VendedorFoto = a.IdUsuarioNavigation?.FotoPerfil,

                // Campos preparados para futuras funcionalidades
                VendedorRating = null,

                VendedorVentas = null
            };
        }

        /// <summary>
        /// Crea un nuevo artículo asociado al usuario autenticado.
        /// </summary>
        /// <param name="dto">Datos del artículo a registrar.</param>
        /// <param name="idUsuario">ID del usuario que crea el artículo.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el DTO es nulo.</exception>
        /// <exception cref="ArgumentException">Se lanza si los datos son inválidos.</exception>
        public void CrearArticulos(CreateArticuloDTO dto, int idUsuario)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "El artículo no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(dto.Titulo))
                throw new ArgumentException("El título del artículo es obligatorio.");

            if (dto.Precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            var articulo = new Articulo
            {
                // Se asigna el usuario autenticado (no viene del frontend)
                IdUsuario = idUsuario,
                IdCategoria = dto.IdCategoria,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                EstadoProducto = dto.EstadoProducto,
                Ubicacion = dto.Ubicacion
            };

            // Se obtiene el ID generado por la base de datos
            int idArticuloGenerado = _repo.GuardarArticulos(articulo);

            // Se guardan las imágenes asociadas al artículo
            foreach (var url in dto.Imagenes)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var imagen = new ImagenesArticulo
                    {
                        IdArticulo = idArticuloGenerado,
                        UrlImagen = url
                    };
                    _imagenRepo.Guardar(imagen);
                }
            }
        }

        /// <summary>
        /// Cambia el estado de publicación de un artículo.
        /// </summary>
        /// <param name="idArticulo">ID del artículo.</param>
        /// <param name="estado">Nuevo estado (activo, vendido, etc).</param>
        public void CambiarEstado(int idArticulo, string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new Exception("Estado inválido");

            _repo.ActualizarEstado(idArticulo, estado);
        }

        /// <summary>
        /// Elimina un artículo del sistema.
        /// </summary>
        /// <param name="id">ID del artículo a eliminar.</param>
        public void EliminarArticulo(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del artículo debe ser mayor a 0.", nameof(id));

            _repo.Eliminar(id);
        }

        /// <summary>
        /// Obtiene todos los artículos publicados por un usuario específico.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns>Lista de artículos asociados al usuario.</returns>
        public List<ArticuloDTO> ObtenerPorUsuario(int idUsuario)
        {
            return _repo.ObtenerPorUsuario(idUsuario)
                .Select(a => new ArticuloDTO
                {
                    IdArticulo = a.IdArticulo,
                    Titulo = a.Titulo,
                    Precio = a.Precio,
                    EstadoProducto = a.EstadoProducto,
                    EstadoPublicacion = a.EstadoPublicacion,
                    Descripcion = a.Descripcion,
                    Ubicacion = a.Ubicacion,
                    UrlImagen = a.ImagenesArticulos
                                         .Select(i => i.UrlImagen)
                                         .FirstOrDefault()
                }).ToList();
        }
    }
}
