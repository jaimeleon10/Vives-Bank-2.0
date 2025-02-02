using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.Utils.Pagination;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.Clientes;

/// <summary>
/// Clase donde se implementa la función para obtener un ejemplo de PageResponse de ClienteResponse
/// </summary>
public sealed class PageResponseClienteExample : IExamplesProvider<PageResponse<ClienteResponse>>
{
    /// <summary>
    /// Función para obtener un ejemplo de PageResponse de ClienteResponse
    /// </summary>
    /// <returns>Devuelve un ejemplo de PageResponse de ClienteResponse</returns>
    public PageResponse<ClienteResponse> GetExamples()
    {
        var pageResponseCliente = new PageResponse<ClienteResponse>();
        pageResponseCliente.Content = new List<ClienteResponse>{
                new ClienteResponse
                {
                    Guid = "123456at",
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
                        Role = "Admin",
                        CreatedAt = "2021-10-10",
                        UpdatedAt = "2021-10-10",
                        IsDeleted = false  
                    },
                    CreatedAt = "2021-10-10",
                    UpdatedAt = "2021-10-10",
                    IsDeleted = false
                }
            };
        pageResponseCliente.TotalPages = 1;
        pageResponseCliente.TotalElements = 1;
        pageResponseCliente.PageSize = 1;
        pageResponseCliente.PageNumber = 1;
        pageResponseCliente.Empty = false;
        pageResponseCliente.First = true;
        pageResponseCliente.Last = true;
        pageResponseCliente.SortBy = "Guid";
        pageResponseCliente.Direction = "ASC";
            
        return pageResponseCliente;
    }
}