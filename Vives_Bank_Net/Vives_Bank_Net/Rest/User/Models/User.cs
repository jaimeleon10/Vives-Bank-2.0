using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vives_Bank_Net.Utils.Generators;

namespace Vives_Bank_Net.Rest.User.Models;

public class User 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string Guid { get; set; } = GuidGenerator.GenerarId();
    
    [Required]
    [Column(TypeName = "nvarchar(100)")]
    public  string UserName { get; set; }
    
    [Required]
    [MinLength(5, ErrorMessage = "Password debe tener al menos 5 caracteres")]
    public  string PasswordHash { get; set; }
    
    [Required]
    public Role Role { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Required]
    public bool IsDeleted { get; set; } = false;
}