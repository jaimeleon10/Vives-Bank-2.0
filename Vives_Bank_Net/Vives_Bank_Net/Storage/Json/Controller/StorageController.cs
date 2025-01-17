using Microsoft.AspNetCore.Mvc;
using Vives_Bank_Net.Rest.Cliente.Models;
using Vives_Bank_Net.Storage.Json.Service;
using Vives_Bank_Net.Rest.User;
using Vives_Bank_Net.Rest.User.Models;

namespace Vives_Bank_Net.Storage.Json.Controller;

// Este controller solo es de prueba, las funciones definitivas estaran dentro de cada uno de sus 
[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IStorageJson _storageJson;
    private readonly ILogger<StorageController> _logger;

    public StorageController(IStorageJson storageJson, ILogger<StorageController> logger)
    {
        _storageJson = storageJson;
        _logger = logger;
    }

    [HttpPost("export-users")]
    public IActionResult ExportUsers()
    {
        try
        {
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Guid = Guid.NewGuid().ToString(),
                    UserName = "Usuario1",
                    Password = "Password123",
                    Role = Role.ADMIN,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new User
                {
                    Id = 2,
                    Guid = Guid.NewGuid().ToString(),
                    UserName = "Usuario2",
                    Password = "Password456",
                    Role = Role.USER,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            var file = new FileInfo("./data/users.json");
            _storageJson.ExportJson(file, users);

            return Ok("Usuarios exportados correctamente a users.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar usuarios");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("import-users")]
    public IActionResult ImportUsers()
    {
        try
        {
            var file = new FileInfo("users.json");
            if (!file.Exists)
            {
                return NotFound("El archivo users.json no existe");
            }

            var users = _storageJson.ImportJson<User>(file);

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar usuarios");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    
    [HttpPost("export-clientes")]
    public IActionResult ExportClientes()
    {
        try
        {
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    Nombre = "Mario",
                    Apellidos = "de Domingo Alvarez",
                    Direccion = new Direccion()
                    {
                        Calle = "Calle 1",
                        Numero = "123",
                        CodigoPostal = "12345",
                        Piso = "1",
                        Letra = "A"
                    },
                    Email = "mario@example.com",
                    Telefono = "666777889",
                    FotoPerfil = "foto_mario.jpg",
                    FotoDni = "foto_dni_mario.jpg",
                    User = new User
                    {
                        Id = 1,
                        Guid = Guid.NewGuid().ToString(),
                        UserName = "Usuario1",
                        Password = "Password123",
                        Role = Role.ADMIN,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    }
                },
                new Cliente
                {
                    Nombre = "Mario",
                    Apellidos = "de Domingo Alvarez",
                    Direccion = new Direccion()
                    {
                        Calle = "Calle 1",
                        Numero = "123",
                        CodigoPostal = "12345",
                        Piso = "1",
                        Letra = "A"
                    },
                    Email = "mario@example.com",
                    Telefono = "666777889",
                    FotoPerfil = "foto_mario.jpg",
                    FotoDni = "foto_dni_mario.jpg",
                    User = new User
                    {
                        Id = 1,
                        Guid = Guid.NewGuid().ToString(),
                        UserName = "Usuario1",
                        Password = "Password123",
                        Role = Role.ADMIN,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    }
                }
            };
            var file = new FileInfo("./data/clientes.json");
            _storageJson.ExportJson(file, clientes);

            return Ok("Usuarios exportados correctamente a users.json");
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar usuarios");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}