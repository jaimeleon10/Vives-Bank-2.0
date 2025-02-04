using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Banco_VivesBank.Cliente.Models;

/// <summary>
/// Dirección de un cliente
/// </summary>
[Owned] 
public class Direccion
{
    [Required(ErrorMessage = "La calle no puede estar vacía")]
    [MaxLength(150, ErrorMessage = "La calle debe tener como máximo 150 caracteres")]
    [JsonPropertyName("calle")]
    public string Calle { get; set; }

    [Required(ErrorMessage = "El número de calle no puede estar vacio")]
    [MaxLength(5, ErrorMessage = "El numero de la direccion debe tener como máximo 4 caracteres")]
    [JsonPropertyName("numero")]
    public string Numero { get; set; }

    [Required(ErrorMessage = "El código postal no puede estar vacío")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 números")]
    [JsonPropertyName("codigoPostal")]
    public string CodigoPostal { get; set; }

    [Required(ErrorMessage = "El piso no puede estar vacio")]
    [MaxLength(3, ErrorMessage = "El piso debe tener como máximo 3 caracteres")]
    [JsonPropertyName("piso")]
    public string Piso { get; set; }

    [Required(ErrorMessage = "La letra no puede estar vacia")]
    [MaxLength(2, ErrorMessage = "La letra de la direccion debe tener como máximo 2 caracteres")]
    [JsonPropertyName("letra")]
    public string Letra { get; set; }
}