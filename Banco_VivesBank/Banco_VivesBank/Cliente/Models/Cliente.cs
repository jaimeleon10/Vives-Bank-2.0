using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Banco_VivesBank.Producto.Cuenta.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.Cliente.Models;

/// <summary>
///  Representa un cliente del banco
/// </summary>
public class Cliente
{
    public long Id { get; set; }
    
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    public string Dni { get; set; }

    public string Nombre { get; set; }

    public string Apellidos { get; set; }
    
    public Direccion Direccion { get; set; }

    public string Email { get; set; }

    public string Telefono { get; set; }
    
    public string FotoPerfil { get; set; } = "https://example.com/fotoPerfil.jpg";

    public string FotoDni { get; set; } = "https://example.com/fotoDni.jpg";

    public IEnumerable<Cuenta> Cuentas { get; set; } = new HashSet<Cuenta>();

    public User.Models.User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}