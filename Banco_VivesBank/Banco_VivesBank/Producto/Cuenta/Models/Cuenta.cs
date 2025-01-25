using System.Numerics;
using System.Text.Json.Serialization;
using Banco_VivesBank.Producto.Tarjeta.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Producto.Cuenta.Models;


public class Cuenta
{
    public long Id { get; set; } = 0;
    
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    public string Iban { get; set; } = IbanGenerator.GenerateIban();

    public BigInteger Saldo { get; set; } = 0;
    
    public Tarjeta.Models.Tarjeta? Tarjeta { get; set; } = null;
    
    public Cliente.Models.Cliente Cliente { get; set; }
    
    public ProductoBase.Models.Producto Producto { get; set; }

    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
}