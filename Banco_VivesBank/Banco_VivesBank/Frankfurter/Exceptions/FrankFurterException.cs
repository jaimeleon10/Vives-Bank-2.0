using System.Runtime.Serialization;

namespace Banco_VivesBank.Frankfurter.Exceptions;

public abstract class FrankFurterException : Exception
{
    protected FrankFurterException(string message) : base(message) { }

    protected FrankFurterException(string message, Exception innerException) : base(message, innerException) { }

}