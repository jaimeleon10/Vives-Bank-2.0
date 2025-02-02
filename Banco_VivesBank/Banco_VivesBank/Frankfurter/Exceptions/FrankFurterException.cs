namespace Banco_VivesBank.Frankfurter.Exceptions;

/// <summary>
/// Clase base para excepciones personalizadas relacionadas con el servicio FrankFurter.
/// </summary>
public abstract class FrankFurterException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase `FrankFurterException` con un mensaje de error especificado.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    protected FrankFurterException(string message) : base(message) { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase `FrankFurterException` con un mensaje de error especificado 
    /// y una excepción interna que representa la causa original de la excepción.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    /// <param name="innerException">La excepción interna que representa la causa original de la excepción.</param>
    protected FrankFurterException(string message, Exception innerException) : base(message, innerException) { }

}