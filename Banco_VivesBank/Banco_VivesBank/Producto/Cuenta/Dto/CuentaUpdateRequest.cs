using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Producto.Cuenta.Dto;

public class CuentaUpdateRequest
{
    [Required(ErrorMessage = "El campo Saldo es obligatorio")]
    public string Saldo { get; set; }
    
    [Required(ErrorMessage = "El campo clienteId es obligatorio")]
    public string ClienteGuid { get; set; }
}