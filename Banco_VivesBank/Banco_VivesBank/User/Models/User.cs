using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.User.Models;
/// <summary>
///  Representa el usuario de un banco
/// </summary>
public class User 
{
    public long Id { get; set; }

    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    public  string Username { get; set; }
    
    public  string Password { get; set; }

    public Role Role { get; set; } = Role.User;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}