using Banco_VivesBank.Producto.ProductoBase.Dto;
using Banco_VivesBank.Utils.Pagination;

namespace Banco_VivesBank.Producto.ProductoBase.Services
{
    /// <summary>
    /// Define los métodos para gestionar los productos en el sistema.
    /// </summary>
    /// <remarks>
    /// La interfaz <c>IProductoService</c> define las operaciones básicas que pueden realizarse sobre los productos,
    /// como obtener productos paginados, obtener un producto por su identificador o tipo, crear, actualizar, eliminar productos,
    /// y obtener los modelos básicos de productos para el almacenamiento.
    /// </remarks>
    public interface IProductoService
    {
        /// <summary>
        /// Obtiene una lista de productos paginada según los parámetros de la solicitud.
        /// </summary>
        /// <param name="pageRequest">Los parámetros de paginación y ordenación.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo PageResponse<ProductoResponse> que contiene los productos paginados.</returns>
        public Task<PageResponse<ProductoResponse>> GetAllPagedAsync(PageRequest pageRequest);

        /// <summary>
        /// Obtiene un producto por su GUID.
        /// </summary>
        /// <param name="guid">El GUID del producto.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoResponse que contiene el producto.</returns>
        public Task<ProductoResponse?> GetByGuidAsync(string guid);

        /// <summary>
        /// Obtiene un producto por su tipo.
        /// </summary>
        /// <param name="tipo">El tipo de producto.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoResponse que contiene el producto.</returns>
        public Task<ProductoResponse?> GetByTipoAsync(string tipo);

        /// <summary>
        /// Crea un nuevo producto en el sistema.
        /// </summary>
        /// <param name="productoRequest">Los datos del producto a crear.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoResponse que contiene el producto creado.</returns>
        public Task<ProductoResponse> CreateAsync(ProductoRequest productoRequest);

        /// <summary>
        /// Actualiza los datos de un producto existente.
        /// </summary>
        /// <param name="guid">El GUID del producto que se va a actualizar.</param>
        /// <param name="productoRequestUpdate">Los datos actualizados del producto.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoResponse que contiene el producto actualizado.</returns>
        public Task<ProductoResponse?> UpdateAsync(string guid, ProductoRequestUpdate productoRequestUpdate);

        /// <summary>
        /// Elimina un producto por su GUID.
        /// </summary>
        /// <param name="guid">El GUID del producto a eliminar.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoResponse que contiene el producto eliminado.</returns>
        public Task<ProductoResponse?> DeleteByGuidAsync(string guid);

        /// <summary>
        /// Obtiene el modelo básico de producto a partir de su GUID.
        /// </summary>
        /// <param name="guid">El GUID del producto.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoBase.Models.Producto que contiene los datos del producto.</returns>
        public Task<ProductoBase.Models.Producto?> GetBaseModelByGuid(string guid);

        /// <summary>
        /// Obtiene el modelo básico de producto a partir de su ID.
        /// </summary>
        /// <param name="id">El ID del producto.</param>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo ProductoBase.Models.Producto que contiene los datos del producto.</returns>
        public Task<ProductoBase.Models.Producto?> GetBaseModelById(long id);

        /// <summary>
        /// Obtiene todos los productos para el almacenamiento (sin tener en cuenta el estado de eliminación).
        /// </summary>
        /// <returns>Una tarea que representa la operación asincrónica, con un resultado de tipo IEnumerable<ProductoBase.Models.Producto> que contiene todos los productos.</returns>
        public Task<IEnumerable<ProductoBase.Models.Producto>> GetAllForStorage();
    }
}
