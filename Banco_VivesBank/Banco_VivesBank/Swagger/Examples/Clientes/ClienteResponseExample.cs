using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.User.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Clientes;

public sealed class ClienteResponseExample : IExamplesProvider<ClienteResponse>
{
    public ClienteResponse GetExamples()
    {
        return new ClienteResponse
        {
            Guid = "1t2gVegRt2x",
            Dni = "00000000T",
            Nombre = "John",
            Apellidos = "Doe",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa",
                Numero = "123",
                Piso = "1",
                Letra = "A",
                CodigoPostal = "12345"
            },
            Email = "example@example.com",
            Telefono = "623456789",
            FotoPerfil = "https://example.jpg",
            FotoDni = "https://example.jpg",
            UserResponse = new UserResponse
            {
                Guid = "123456at",
                Username = "JohnDoe",
                Password = "password",
                Role = "Cliente",
                CreatedAt = "2021-10-10",
                UpdatedAt = "2021-10-10",
                IsDeleted = false  
            },
            CreatedAt = "2021-10-10",
            UpdatedAt = "2021-10-10",
            IsDeleted = false
        };
    }
}