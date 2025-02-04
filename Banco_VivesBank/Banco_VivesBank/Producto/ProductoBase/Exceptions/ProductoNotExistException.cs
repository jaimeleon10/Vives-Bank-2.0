namespace Banco_VivesBank.Producto.ProductoBase.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando se intenta acceder a un producto que no existe en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta excepción se utiliza cuando se realiza una operación (como obtener, actualizar o eliminar) en un producto 
    /// que no se encuentra registrado en la base de datos o en el sistema.
    /// </remarks>
    public class ProductoNotExistException : ProductoException
    {
        /// <summary>
        /// Constructor de la excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que describe el motivo de la excepción.</param>
        public ProductoNotExistException(string message) : base(message) { }
    }
}