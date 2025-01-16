using System.ComponentModel.DataAnnotations;

namespace Vives_Bank_Net.Rest.Cliente.Models;

public class Direccion
{
    [Required(ErrorMessage = "La calle no puede estar vacia")]
    public string Calle { get; set; }

    [Required(ErrorMessage = "El número no puede estar vacio")]
    public string Numero { get; set; }

    [Required(ErrorMessage = "El código postal no puede estar vacío")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 números")]
    public string CodigoPostal { get; set; }

    [Required(ErrorMessage = "El piso no puede estar vacio")]
    public string Piso { get; set; }

    [Required(ErrorMessage = "La letra no puede estar vacia")]
    public string Letra { get; set; }
}