namespace Banco_VivesBank.Producto.ProductoBase.Exceptions
{
    /// <summary>
    /// Excepción base para errores relacionados con los productos en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta clase es la base de todas las excepciones específicas relacionadas con productos. 
    /// Se utiliza para manejar errores generales o comunes que puedan ocurrir durante el procesamiento de productos.
    /// </remarks>
    public class ProductoException : Exception
    {
        /// <summary>
        /// Constructor de la excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que describe el motivo de la excepción.</param>
        public ProductoException(string message) : base(message) { }
    }
}