using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Mapper;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Models;
using NUnit.Framework;

namespace Test.Cliente.Mapper;

[TestFixture]
public class ClienteMapperTest
{
     [Test]
    public void ToModelFromEntity()
    {
        var clienteEntity = new ClienteEntity
        {
            Guid = "guid",
            Dni = "12345678Z",
            Nombre = "test",
            Apellidos = "testApe",
            Direccion = new Banco_VivesBank.Cliente.Models.Direccion
            {
                Calle = "testCalle",
                Numero = "1",
                CodigoPostal = "12345",
                Piso = "1",
                Letra = "A",
            },
            Email = "algo@test.com",
            Telefono = "123456789",
            FotoPerfil = "perfil.jpg",
            FotoDni = "dni.jpg",
            UserId = 1,
            User = new UserEntity
            {
                Id = 1,
                Guid = "user-guid",
                Username = "test",
                Password = "test",
                Role = Role.User
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ClienteMapper.ToModelFromEntity(clienteEntity);
        
        Assert.Multiple(() =>
        {
            Assert.That(clienteEntity.Dni, Is.EqualTo(result.Dni));
            Assert.That(clienteEntity.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(clienteEntity.Nombre, Is.EqualTo(result.Nombre));
            Assert.That(clienteEntity.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(clienteEntity.Direccion.Calle, Is.EqualTo(result.Direccion.Calle));
            Assert.That(clienteEntity.Direccion.Numero, Is.EqualTo(result.Direccion.Numero));
            Assert.That(clienteEntity.Direccion.CodigoPostal, Is.EqualTo(result.Direccion.CodigoPostal));
            Assert.That(clienteEntity.Direccion.Piso, Is.EqualTo(result.Direccion.Piso));
            Assert.That(clienteEntity.Direccion.Letra, Is.EqualTo(result.Direccion.Letra));
            Assert.That(clienteEntity.Email, Is.EqualTo(result.Email));
            Assert.That(clienteEntity.Telefono, Is.EqualTo(result.Telefono));
            Assert.That(clienteEntity.User.Id, Is.EqualTo(result.User.Id));
            Assert.That(clienteEntity.User.Guid, Is.EqualTo(result.User.Guid));
            Assert.That(clienteEntity.User.Username, Is.EqualTo(result.User.Username));
            Assert.That(clienteEntity.User.CreatedAt, Is.EqualTo(result.User.CreatedAt));
            Assert.That(clienteEntity.User.UpdatedAt, Is.EqualTo(result.User.UpdatedAt));
            Assert.That(clienteEntity.User.IsDeleted, Is.EqualTo(result.User.IsDeleted));
            
        });
    }
    
    [Test]
    public void ToModelFromRequest()
    {
        var clienteRequest = new ClienteRequest
        {
            Dni = "12345678Z",
            Nombre = "test",
            Apellidos = "testApe",
            Calle = "testCalle",
            Numero = "1",
            CodigoPostal = "12345",
            Piso = "1",
            Letra = "A",
            Email = "example@ex.com",
            Telefono = "123456789",
        };
        var user = new Banco_VivesBank.User.Models.User
        {
            Guid = "user-guid",
            Username = "test",
            Password ="test",
            Role = Role.User
        };
            
        var result = ClienteMapper.ToModelFromRequest(clienteRequest, user);

        Assert.Multiple(() =>
        {
            Assert.That(clienteRequest.Dni, Is.EqualTo(result.Dni));
            Assert.That(clienteRequest.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(clienteRequest.Nombre, Is.EqualTo(result.Nombre));
            Assert.That(clienteRequest.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(clienteRequest.Calle, Is.EqualTo(result.Direccion.Calle));
            Assert.That(clienteRequest.Numero, Is.EqualTo(result.Direccion.Numero));
            Assert.That(clienteRequest.CodigoPostal, Is.EqualTo(result.Direccion.CodigoPostal));
            Assert.That(clienteRequest.Piso, Is.EqualTo(result.Direccion.Piso));
            Assert.That(clienteRequest.Letra, Is.EqualTo(result.Direccion.Letra));
            Assert.That(clienteRequest.Email, Is.EqualTo(result.Email));
            Assert.That(clienteRequest.Telefono, Is.EqualTo(result.Telefono));
            Assert.That(user, Is.EqualTo(result.User));
        });
    }

    [Test]
    public void ToEntityFromModel()
    {
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Id = 1,
            Guid = "guid",
            Dni = "12345678Z",
            Nombre = "test",
            Apellidos = "testApe",
            Direccion = new Banco_VivesBank.Cliente.Models.Direccion
            {
                Calle = "testCalle",
                Numero = "1",
                CodigoPostal = "12345",
                Piso = "1",
                Letra = "A",
            },
            Email = "example@ex.com",
            Telefono = "123456789",
            FotoPerfil = "perfil.jpg",
            FotoDni = "dni.jpg",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "test",
                Password = "test",
                Role = Role.User
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ClienteMapper.ToEntityFromModel(cliente);

        Assert.Multiple(() =>
        {
            Assert.That(cliente.Id, Is.EqualTo(result.Id));
            Assert.That(cliente.Guid, Is.EqualTo(result.Guid));
            Assert.That(cliente.Dni, Is.EqualTo(result.Dni));
            Assert.That(cliente.Nombre, Is.EqualTo(result.Nombre));
            Assert.That(cliente.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(cliente.Direccion, Is.EqualTo(result.Direccion));
            Assert.That(cliente.Email, Is.EqualTo(result.Email));
            Assert.That(cliente.Telefono, Is.EqualTo(result.Telefono));
            Assert.That(cliente.FotoPerfil, Is.EqualTo(result.FotoPerfil));
            Assert.That(cliente.FotoDni, Is.EqualTo(result.FotoDni));
            Assert.That(cliente.User.Id, Is.EqualTo(result.UserId));
            Assert.That(cliente.CreatedAt, Is.EqualTo(result.CreatedAt));
            Assert.That(cliente.UpdatedAt, Is.EqualTo(result.UpdatedAt));
            Assert.That(cliente.IsDeleted, Is.EqualTo(result.IsDeleted));
        });
    }

    [Test]
    public void ToResponseFromModel()
    {
        var cliente = new Banco_VivesBank.Cliente.Models.Cliente
        {
            Guid = "guid",
            Dni = "12345678Z",
            Nombre = "test",
            Apellidos = "testApe",
            Direccion = new Banco_VivesBank.Cliente.Models.Direccion
            {
                Calle = "testCalle",
                Numero = "1",
                CodigoPostal = "12345",
                Piso = "1",
                Letra = "A",
            },
            Email = "test@test.com",
            Telefono = "123456789",
            FotoPerfil = "perfil.jpg",
            FotoDni = "dni.jpg",
            User = new Banco_VivesBank.User.Models.User
            {
                Guid = "user-guid",
                Username = "test",
                Password = "test",
                Role = Role.User
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = ClienteMapper.ToResponseFromModel(cliente);
        Assert.Multiple(() =>
        {
            Assert.That(cliente.Guid, Is.EqualTo(result.Guid));
            Assert.That(cliente.Dni, Is.EqualTo(result.Dni));
            Assert.That(cliente.Nombre, Is.EqualTo(result.Nombre));
            Assert.That(cliente.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(cliente.Direccion, Is.EqualTo(result.Direccion));
            Assert.That(cliente.Email, Is.EqualTo(result.Email));
            Assert.That(cliente.Telefono, Is.EqualTo(result.Telefono));
            Assert.That(cliente.FotoPerfil, Is.EqualTo(result.FotoPerfil));
            Assert.That(cliente.FotoDni, Is.EqualTo(result.FotoDni));
        });
    }
    
    [Test]
    public void ToResponseFromEntity()
    {
        var user = new UserEntity
        {
            Id = 1,
            Guid = "user-guid",
            Username = "test",
            Password = "test",
            Role = Role.User
        };
        var clienteEntity = new ClienteEntity
        {
            Guid = "guid",
            Dni = "12345678Z",
            Nombre = "test",
            Apellidos = "testApe",
            Direccion = new Banco_VivesBank.Cliente.Models.Direccion
            {
                Calle = "testCalle",
                Numero = "1",
                CodigoPostal = "12345",
                Piso = "1",
                Letra = "A",
            },
            Email = "test@test.com",
            Telefono = "123456789",
            FotoPerfil = "perfil.jpg",
            FotoDni = "dni.jpg",
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var result = ClienteMapper.ToResponseFromEntity(clienteEntity);
        Assert.Multiple(() =>
        {
            Assert.That(clienteEntity.Guid, Is.EqualTo(result.Guid));
            Assert.That(clienteEntity.Dni, Is.EqualTo(result.Dni));
            Assert.That(clienteEntity.Nombre, Is.EqualTo(result.Nombre));
            Assert.That(clienteEntity.Apellidos, Is.EqualTo(result.Apellidos));
            Assert.That(clienteEntity.Direccion, Is.EqualTo(result.Direccion));
            Assert.That(clienteEntity.Email, Is.EqualTo(result.Email));
            Assert.That(clienteEntity.Telefono, Is.EqualTo(result.Telefono));
            Assert.That(clienteEntity.FotoPerfil, Is.EqualTo(result.FotoPerfil));
            Assert.That(clienteEntity.FotoDni, Is.EqualTo(result.FotoDni));
            Assert.That(clienteEntity.UserId, Is.EqualTo(user.Id));
        });
    }
}
