using System.ComponentModel.DataAnnotations;

namespace Vives_Bank_Net.Rest.Producto.Cuenta.Dto;

public class CuentaRequest
{
    public required string TipoCuenta { get; set; }
}