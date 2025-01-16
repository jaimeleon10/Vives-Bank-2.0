namespace Vives_Bank_Net.Rest.Cliente.Dtos;

public class ClienteResponse
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Calle  { get; set; }
    public string Numero  { get; set; }
    public string CodigoPostal { get; set; }
    public string Piso  { get; set; }
    public string Letra  { get; set; }
    public string Email { get; set; }
    public string Telefono  { get; set; }
    public string FotoPerfil  { get; set; }
    public string FotoDni  { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
}