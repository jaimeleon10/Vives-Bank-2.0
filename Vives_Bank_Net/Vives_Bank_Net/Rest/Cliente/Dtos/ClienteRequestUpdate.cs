namespace Vives_Bank_Net.Rest.Cliente.Dtos;

public class ClienteRequestUpdate
{
    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Calle  { get; set; }
    public string Numero  { get; set; }
    public string CodigoPostal { get; set; }
    public string Piso  { get; set; }
    public string Letra  { get; set; }
    public string Email  { get; set; }
    public string Telefono  { get; set; }
    public string FotoPerfil  { get; set; }
    public string FotoDni  { get; set; }
}