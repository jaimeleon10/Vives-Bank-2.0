using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.User.Models;

public class User 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [Required]
    public  string Username { get; set; }
    
    [Required]
    [MinLength(5, ErrorMessage = "Password debe tener al menos 5 caracteres")]
    public  string Password { get; set; }
    
    [Required]
    public Role Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}