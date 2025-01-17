using System.Runtime.Serialization;

namespace Vives_Bank_Net.Frankfurter.Exceptions;

public abstract class FrankFurterException : Exception
{
    protected FrankFurterException(string message) : base(message) { }

    protected FrankFurterException(string message, Exception innerException) : base(message, innerException) { }

    protected FrankFurterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}