using System.ComponentModel.DataAnnotations;

namespace Banco_VivesBank.User.Dto;

public class UserRequestUpdate
{
    public string Role { get; set; } = Models.Role.User.GetType().ToString();

    public bool IsDeleted { get; set; } = false;
}