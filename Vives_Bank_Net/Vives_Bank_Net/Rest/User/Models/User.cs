using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Vives_Banks_Net.Utils.Generators;

namespace Vives_Banks_Net.Rest.User;

public class User : IdentityUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [Required]
    [Column(TypeName = "nvarchar(100)")]
    public override string UserName { get; set; }
    
    [Required]
    [MinLength(5, ErrorMessage = "Password debe tener al menos 5 caracteres")]
    public override string PasswordHash { get; set; }
    
    [Required]
    public Role Role { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsDeleted { get; set; } = false;
}