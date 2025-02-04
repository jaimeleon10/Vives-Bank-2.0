namespace Banco_VivesBank.Producto.ProductoBase.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando se intenta crear un producto con un nombre que ya existe en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta excepción se utiliza para manejar casos en los que se intenta registrar o crear un producto con un nombre que ya está registrado en el sistema.
    /// </remarks>
    public class ProductoExistByNameException : ProductoException
    {
        /// <summary>
        /// Constructor de la excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que describe el motivo de la excepción.</param>
        public ProductoExistByNameException(string message) : base(message) { }
    }
}