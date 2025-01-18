namespace Banco_VivesBank.User.Dto;

public class UserResponse
{
    public string Guid { get; set; } = null!;
    
    public string Username { get; set; } = null!;
    
    public string Role { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}