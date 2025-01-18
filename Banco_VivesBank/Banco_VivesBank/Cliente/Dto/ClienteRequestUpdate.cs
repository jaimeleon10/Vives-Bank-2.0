using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteRequestUpdate
{
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Calle  { get; set; }
    public string? Numero  { get; set; }
    
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe estar formado por 5 números")]
    public string? CodigoPostal { get; set; }
    public string? Piso  { get; set; }
    public string? Letra  { get; set; }
    
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string? Email  { get; set; }
    
    [Phone(ErrorMessage = "El número de teléfono no tiene un formato válido")]
    public string? Telefono  { get; set; }
  //public string? FotoPerfil  { get; set; }
  //public string? FotoDni  { get; set; }
  
  public bool HasAtLeastOneField()
  {
      return new[] { Nombre, Apellidos, Calle, Numero, CodigoPostal, Piso, Letra, Email, Telefono/*, FotoPerfil, FotoDni*/ }
          .Any(field => !string.IsNullOrWhiteSpace(field));
  }
}