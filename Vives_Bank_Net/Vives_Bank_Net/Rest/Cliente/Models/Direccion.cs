using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Vives_Bank_Net.Rest.Cliente.Models;

[Owned]
public class Direccion
{
    [Required(ErrorMessage = "La calle no puede estar vacia")]
    [JsonPropertyName("calle")]
    public string Calle { get; set; }

    [Required(ErrorMessage = "El número no puede estar vacio")]
    [JsonPropertyName("numero")]
    public string Numero { get; set; }

    [Required(ErrorMessage = "El código postal no puede estar vacío")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 números")]
    [JsonPropertyName("codigoPostal")]
    public string CodigoPostal { get; set; }

    [Required(ErrorMessage = "El piso no puede estar vacio")]
    [JsonPropertyName("piso")]
    public string Piso { get; set; }

    [Required(ErrorMessage = "La letra no puede estar vacia")]
    [JsonPropertyName("letra")]
    public string Letra { get; set; }
}