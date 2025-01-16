using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vives_Bank_Net.Rest.User.Database
{
    [Table("Users")]
    public class UserEntity
    {
        public const long NewId = 0;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } = NewId;

        [Required]
        public string Guid { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string UserName { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}