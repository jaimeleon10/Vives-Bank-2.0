namespace Banco_VivesBank.Producto.ProductoBase.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando un producto ya existe en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta excepción se utiliza cuando se intenta crear un producto con un identificador o características
    /// que ya existen en la base de datos o el sistema, como por ejemplo un nombre duplicado, un código de producto ya registrado, etc.
    /// </remarks>
    public class ProductoDuplicatedException : ProductoException
    {
        /// <summary>
        /// Constructor de la excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que describe el motivo de la excepción.</param>
        public ProductoDuplicatedException(string message) : base(message) { }
    }
}