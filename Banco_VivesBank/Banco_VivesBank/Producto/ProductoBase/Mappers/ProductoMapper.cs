using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.ProductoBase.Dto;

namespace Banco_VivesBank.Producto.ProductoBase.Mappers
{
    /// <summary>
    /// Contiene métodos estáticos para mapear entre las entidades, modelos y respuestas de los productos.
    /// </summary>
    /// <remarks>
    /// Esta clase proporciona métodos que permiten convertir entre las diferentes representaciones de los productos en el sistema:
    /// - Desde la solicitud del producto (`ProductoRequest`) a su modelo de datos (`ProductoBase.Models.Producto`).
    /// - Desde la entidad de base de datos (`ProductoEntity`) al modelo de datos.
    /// - Desde el modelo de datos a la respuesta que se retorna a la API (`ProductoResponse`).
    /// </remarks>
    public static class ProductoMapper
    {
        /// <summary>
        /// Convierte un objeto de solicitud de producto a un modelo de producto.
        /// </summary>
        /// <param name="d">El objeto de solicitud de producto a convertir.</param>
        /// <returns>Un objeto de tipo ProductoBase.Models.Producto.</returns>
        public static ProductoBase.Models.Producto ToModelFromRequest(this ProductoRequest d)
        {
            return new ProductoBase.Models.Producto
            {
                Nombre = d.Nombre,
                Descripcion = d.Descripcion,
                Tae = d.Tae,
                TipoProducto = d.TipoProducto,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = d.IsDeleted
            };
        }

        /// <summary>
        /// Convierte una entidad de producto de base de datos a un modelo de producto.
        /// </summary>
        /// <param name="entity">La entidad de producto a convertir.</param>
        /// <returns>Un objeto de tipo ProductoBase.Models.Producto.</returns>
        public static ProductoBase.Models.Producto ToModelFromEntity(this ProductoEntity entity)
        {
            return new ProductoBase.Models.Producto
            {
                Id = entity.Id,
                Guid = entity.Guid,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Tae = entity.Tae,
                TipoProducto = entity.TipoProducto,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.IsDeleted
            };
        }

        /// <summary>
        /// Convierte un modelo de producto a su correspondiente entidad de base de datos.
        /// </summary>
        /// <param name="model">El modelo de producto a convertir.</param>
        /// <returns>Una entidad de tipo ProductoEntity.</returns>
        public static ProductoEntity ToEntityFromModel(this ProductoBase.Models.Producto model)
        {
            return new ProductoEntity
            {
                Id = model.Id,
                Guid = model.Guid,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Tae = model.Tae,
                TipoProducto = model.TipoProducto,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                IsDeleted = model.IsDeleted
            };
        }

        /// <summary>
        /// Convierte un modelo de producto a una respuesta de producto que se devuelve por la API.
        /// </summary>
        /// <param name="model">El modelo de producto a convertir.</param>
        /// <returns>Un objeto de tipo ProductoResponse.</returns>
        public static ProductoResponse ToResponseFromModel(this ProductoBase.Models.Producto model)
        {
            return new ProductoResponse
            {
                Nombre = model.Nombre,
                Guid = model.Guid,
                Descripcion = model.Descripcion,
                Tae = model.Tae,
                TipoProducto = model.TipoProducto,
                CreatedAt = model.CreatedAt.ToString(),
                UpdatedAt = model.UpdatedAt.ToString(),
                IsDeleted = model.IsDeleted
            };
        }

        /// <summary>
        /// Convierte una entidad de producto de base de datos a una respuesta de producto que se devuelve por la API.
        /// </summary>
        /// <param name="entity">La entidad de producto a convertir.</param>
        /// <returns>Un objeto de tipo ProductoResponse.</returns>
        public static ProductoResponse ToResponseFromEntity(this ProductoEntity entity)
        {
            return new ProductoResponse
            {
                Nombre = entity.Nombre,
                Guid = entity.Guid,
                Descripcion = entity.Descripcion,
                Tae = entity.Tae,
                TipoProducto = entity.TipoProducto,
                CreatedAt = entity.CreatedAt.ToString(),
                UpdatedAt = entity.UpdatedAt.ToString(),
                IsDeleted = entity.IsDeleted
            };
        }

        /// <summary>
        /// Convierte una lista de entidades de productos a una lista de respuestas de productos.
        /// </summary>
        /// <param name="entities">La lista de entidades de productos a convertir.</param>
        /// <returns>Una lista de objetos de tipo ProductoResponse.</returns>
        public static IEnumerable<ProductoResponse> ToResponseListFromEntityList(this IEnumerable<ProductoEntity> entities)
        {
            return entities.Select(entity => ToResponseFromEntity(entity));
        }
    }
}
