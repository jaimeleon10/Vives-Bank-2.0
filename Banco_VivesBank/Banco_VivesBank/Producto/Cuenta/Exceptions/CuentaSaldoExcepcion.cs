namespace Banco_VivesBank.Producto.Cuenta.Exceptions;

public class CuentaSaldoExcepcion(String guid) : CuentaException($"No se puede eliminar la cuenta con el GUID {guid} porque tiene saldo")
{
}