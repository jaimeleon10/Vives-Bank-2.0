using Banco_VivesBank.User.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.User;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de UserResponse
/// </summary>
public sealed class UserResponseExample : IExamplesProvider<UserResponse>
{
    /// <summary>
    /// Función para obtener un ejemplo de UserResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de UserResponse</returns>
    public UserResponse GetExamples()
    {
        return new UserResponse
        {
            Guid = "123456at",
            Username = "JohnDoe",
            Password = "password",
            Role = "Admin",
            CreatedAt = "2021-10-10",
            UpdatedAt = "2021-10-10",
            IsDeleted = false
        };
    }
}