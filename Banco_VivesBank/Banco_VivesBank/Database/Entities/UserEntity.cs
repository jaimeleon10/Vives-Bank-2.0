using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.User.Models;

namespace Banco_VivesBank.Database.Entities;

[Table("Usuarios")]
public class UserEntity
{
    public const long NewId = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; } = NewId;

    [Required]
    public string Guid { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
    
    public Role Role { get; set; } = Role.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [System.ComponentModel.DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}