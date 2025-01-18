using Banco_VivesBank.Cliente.Models;

namespace Banco_VivesBank.Cliente.Dto;

public class ClienteResponse
{
    public string Guid { get; set; }
    
    public string Dni { get; set; }
    
    public string Nombre { get; set; }
    
    public string Apellidos { get; set; }
    
    public Direccion Direccion  { get; set; }
    
    public string Email { get; set; }
    
    public string Telefono  { get; set; }
    
    public string FotoPerfil  { get; set; }
    
    public string FotoDni  { get; set; }
    
    public string UserId { get; set; }
    
    public string CreatedAt { get; set; }
    
    public string UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}