using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Servicies
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repo;

        public CategoriaService(ICategoriaRepository repo)
        { 
            _repo = repo;
        }

        //Metodo para obtner todas la categorias
        public List<Categoria> ObtenerTodas()
        {
            return _repo.ObtenerTodas();
        }

        public void GuardarCategoria(Categoria categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria), "La categoría no puede ser nula.");

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");

            _repo.Guardar(categoria);
        }

        public void ActualizarCategoria(Categoria categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria), "La categoría no puede ser nula.");

            if (categoria.IdCategoria <= 0)
                throw new ArgumentException("El ID de la categoría debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");

            _repo.Actualizar(categoria);
        }

        public void EliminarCategoria(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID de la categoría debe ser mayor a 0.", nameof(id));

            _repo.Eliminar(id);
        }
    }
}
