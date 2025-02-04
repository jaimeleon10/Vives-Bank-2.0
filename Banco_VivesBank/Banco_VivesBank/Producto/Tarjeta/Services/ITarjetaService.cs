using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Utils.Pagination;
using Swashbuckle.AspNetCore.Annotations;

namespace Banco_VivesBank.Producto.Tarjeta.Services;

/// <summary>
    /// Define los métodos para la gestión de tarjetas, incluyendo operaciones de lectura, creación, actualización y eliminación.
    /// </summary>
    public interface ITarjetaService
    {
        /// <summary>
        /// Obtiene una lista paginada de tarjetas.
        /// </summary>
        /// <param name="pageRequest">Detalles de la página solicitada (por ejemplo, número de página y tamaño).</param>
        /// <returns>Una lista paginada de respuestas de tarjetas.</returns>
        [SwaggerOperation(Summary = "Obtiene una lista paginada de tarjetas.")]
        public Task<PageResponse<TarjetaResponse>> GetAllPagedAsync(PageRequest pageRequest);

        /// <summary>
        /// Obtiene una tarjeta basada en su GUID.
        /// </summary>
        /// <param name="guid">El GUID de la tarjeta.</param>
        /// <returns>La respuesta de la tarjeta, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Obtiene una tarjeta basada en su GUID.")]
        public Task<TarjetaResponse?> GetByGuidAsync(string guid);

        /// <summary>
        /// Obtiene una tarjeta basada en su número.
        /// </summary>
        /// <param name="numeroTarjeta">El número de la tarjeta.</param>
        /// <returns>La respuesta de la tarjeta, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Obtiene una tarjeta basada en su número.")]
        public Task<TarjetaResponse?> GetByNumeroTarjetaAsync(string numeroTarjeta);

        /// <summary>
        /// Crea una nueva tarjeta.
        /// </summary>
        /// <param name="tarjetaRequest">Los datos necesarios para crear la tarjeta.</param>
        /// <param name="user">El usuario que realiza la creación.</param>
        /// <returns>La respuesta de la tarjeta recién creada.</returns>
        [SwaggerOperation(Summary = "Crea una nueva tarjeta.")]
        public Task<TarjetaResponse> CreateAsync(TarjetaRequest tarjetaRequest, User.Models.User user);

        /// <summary>
        /// Actualiza una tarjeta existente.
        /// </summary>
        /// <param name="guid">El GUID de la tarjeta a actualizar.</param>
        /// <param name="dto">Los datos a actualizar.</param>
        /// <param name="user">El usuario que realiza la actualización.</param>
        /// <returns>La respuesta de la tarjeta actualizada, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Actualiza una tarjeta existente.")]
        public Task<TarjetaResponse?> UpdateAsync(string guid, TarjetaRequestUpdate dto, User.Models.User user);

        /// <summary>
        /// Elimina una tarjeta existente.
        /// </summary>
        /// <param name="guid">El GUID de la tarjeta a eliminar.</param>
        /// <param name="user">El usuario que realiza la eliminación.</param>
        /// <returns>La respuesta de la tarjeta eliminada, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Elimina una tarjeta existente.")]
        public Task<TarjetaResponse?> DeleteAsync(string guid, User.Models.User user);

        /// <summary>
        /// Obtiene el modelo de la tarjeta basado en su GUID.
        /// </summary>
        /// <param name="guid">El GUID de la tarjeta.</param>
        /// <returns>El modelo de la tarjeta, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Obtiene el modelo de la tarjeta basado en su GUID.")]
        public Task<Models.Tarjeta?> GetTarjetaModelByGuid(string guid);

        /// <summary>
        /// Obtiene el modelo de la tarjeta basado en su ID.
        /// </summary>
        /// <param name="id">El ID de la tarjeta.</param>
        /// <returns>El modelo de la tarjeta, o null si no se encuentra.</returns>
        [SwaggerOperation(Summary = "Obtiene el modelo de la tarjeta basado en su ID.")]
        public Task<Models.Tarjeta?> GetTarjetaModelById(long id);

        /// <summary>
        /// Obtiene todas las tarjetas para almacenamiento.
        /// </summary>
        /// <returns>Una lista de tarjetas.</returns>
        [SwaggerOperation(Summary = "Obtiene todas las tarjetas para almacenamiento.")]
        public Task<List<Models.Tarjeta>> GetAllForStorage();
    }