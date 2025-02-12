namespace Banco_VivesBank.Producto.Cuenta.Exceptions;

public class CuentaNotSerializableExceptions(String message) : CuentaException("No se he podido deserializar la cuenta debido a un error de redis.")
{
    
}