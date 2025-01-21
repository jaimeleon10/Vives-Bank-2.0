using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.Cuenta.Dto;

public class CuentaRequest
{
    [Required(ErrorMessage = "El campo tipoCuenta es obligatorio")]
    public string TipoCuenta { get; set; }
    
    [Required(ErrorMessage = "El campo clienteId es obligatorio")]
    public string ClienteGuid { get; set; }
}