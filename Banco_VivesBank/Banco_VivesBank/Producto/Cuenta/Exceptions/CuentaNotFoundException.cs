namespace Banco_VivesBank.Producto.Cuenta.Exceptions;

public class CuentaNotFoundException(String message): CuentaException($"No se ha encontrado la cuenta con el guuid: {message}"){}
