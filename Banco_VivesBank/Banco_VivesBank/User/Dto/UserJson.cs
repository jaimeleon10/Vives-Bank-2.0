using System.Text.Json.Serialization;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.Utils.Generators;

namespace Banco_VivesBank.User.Dto;

public class UserJson
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("guid")]
    public string Guid { get; set; }
  
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("role")]
    public Role Role { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } 

    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; } 
}