using System.Globalization;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.User.Models;

namespace Test.Producto.Cuenta.Mapper;

[TestFixture]
public class CuentaMapperTests
{
   
    [Test]
    public void ToModelFromEntity()
    {
        var clienteEntity = new ClienteEntity
        {
            Id = 1,
            Guid = "cliente-guid",
            Dni = "12345678",
            Nombre = "Nombre",
            Apellidos = "Apellidos",
            Direccion = new Direccion
                { Calle = "Calle", Numero = "1", CodigoPostal = "12345", Piso = "1", Letra = "A" },
            Email = "email@example.com",
            Telefono = "123456789",
            User = new UserEntity
            {
                Id = 1, Guid = "user-guid", Username = "username", Password = "password", Role = Role.Cliente,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false
            },
            IsDeleted = false
        };
        
        var cuentaEntity = new CuentaEntity
        {
            Id = 1,
            Guid = "guid-cuenta",
            Iban = "ES1234567890",
            Saldo = 1000.0,
            Tarjeta = new TarjetaEntity { Id = 1, Guid = "guid-tarjeta" },
            Cliente = clienteEntity,
            Producto = new ProductoEntity { Id = 1, Guid = "producto-guid" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        Assert.That(cuentaEntity.Cliente, Is.Not.Null);
        Assert.That(cuentaEntity.Cliente?.Guid, Is.Not.Null);

        var result = cuentaEntity.ToModelFromEntity();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(cuentaEntity.Id));
        Assert.That(result.Guid, Is.EqualTo(cuentaEntity.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaEntity.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaEntity.Saldo));
        Assert.That(result.Tarjeta?.Guid, Is.EqualTo(cuentaEntity.Tarjeta?.Guid)); 
        Assert.That(result.Cliente?.Guid, Is.EqualTo(cuentaEntity.Cliente?.Guid)); 
        Assert.That(result.Producto?.Guid, Is.EqualTo(cuentaEntity.Producto?.Guid));
        Assert.That(result.CreatedAt, Is.EqualTo(cuentaEntity.CreatedAt));
        Assert.That(result.UpdatedAt, Is.EqualTo(cuentaEntity.UpdatedAt));
        Assert.That(result.IsDeleted, Is.EqualTo(cuentaEntity.IsDeleted));
    }
   
    [Test]
    public void ToEntityFromModel()
    {
      
        var cuentaModel = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Id = 1,
            Guid = "guid-cuenta",
            Iban = "ES1234567890",
            Saldo = 1000.0,
            Tarjeta = new Banco_VivesBank.Producto.Tarjeta.Models.Tarjeta { Id = 1, Guid = "guid-tarjeta" },
            Cliente = new Banco_VivesBank.Cliente.Models.Cliente { Id = 1, Guid = "cliente-guid" },
            Producto = new Banco_VivesBank.Producto.ProductoBase.Models.Producto { Id = 1, Guid = "producto-guid" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        
        var result = CuentaMapper.ToEntityFromModel(cuentaModel);

       
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(cuentaModel.Id));
        Assert.That(result.Guid, Is.EqualTo(cuentaModel.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaModel.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaModel.Saldo));
        Assert.That(result.TarjetaId, Is.EqualTo(cuentaModel.Tarjeta.Id));
        Assert.That(result.ClienteId, Is.EqualTo(cuentaModel.Cliente.Id));
        Assert.That(result.ProductoId, Is.EqualTo(cuentaModel.Producto.Id));
        Assert.That(result.CreatedAt, Is.EqualTo(cuentaModel.CreatedAt));
        Assert.That(result.UpdatedAt, Is.EqualTo(cuentaModel.UpdatedAt));
        Assert.That(result.IsDeleted, Is.EqualTo(cuentaModel.IsDeleted));
    }
    
    [Test]
    public void ToResponseFromModel()
    {
        var cuentaModel = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Guid = "guid-cuenta",
            Iban = "ES1234567890",
            Saldo = 1000.0,
            Tarjeta = new Banco_VivesBank.Producto.Tarjeta.Models.Tarjeta { Guid = "guid-tarjeta" },
            Cliente = new Banco_VivesBank.Cliente.Models.Cliente { Guid = "cliente-guid" },
            Producto = new Banco_VivesBank.Producto.ProductoBase.Models.Producto { Guid = "producto-guid" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var result = CuentaMapper.ToResponseFromModel(cuentaModel);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cuentaModel.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaModel.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaModel.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaModel.Tarjeta.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(cuentaModel.Cliente.Guid));
        Assert.That(result.ProductoGuid, Is.EqualTo(cuentaModel.Producto.Guid));
        Assert.That(result.CreatedAt, Is.EqualTo(cuentaModel.CreatedAt.ToString()));
        Assert.That(result.UpdatedAt, Is.EqualTo(cuentaModel.UpdatedAt.ToString()));
        Assert.That(result.IsDeleted, Is.EqualTo(cuentaModel.IsDeleted));
    }
    
    [Test]
    public void ToResponseFromEntity()
    {
       
        var cuentaEntity = new CuentaEntity
        {
            Guid = "guid-cuenta",
            Iban = "ES1234567890",
            Saldo = 1000.0,
            Tarjeta = new TarjetaEntity { Guid = "guid-tarjeta" },
            Cliente = new ClienteEntity { Guid = "cliente-guid" },
            Producto = new ProductoEntity { Guid = "producto-guid" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var result = CuentaMapper.ToResponseFromEntity(cuentaEntity);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(cuentaEntity.Guid));
        Assert.That(result.Iban, Is.EqualTo(cuentaEntity.Iban));
        Assert.That(result.Saldo, Is.EqualTo(cuentaEntity.Saldo));
        Assert.That(result.TarjetaGuid, Is.EqualTo(cuentaEntity.Tarjeta.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(cuentaEntity.Cliente.Guid));
        Assert.That(result.ProductoGuid, Is.EqualTo(cuentaEntity.Producto.Guid));
        Assert.That(result.CreatedAt, Is.EqualTo(cuentaEntity.CreatedAt.ToString(CultureInfo.InvariantCulture)));
        Assert.That(result.UpdatedAt, Is.EqualTo(cuentaEntity.UpdatedAt.ToString(CultureInfo.InvariantCulture)));
        Assert.That(result.IsDeleted, Is.EqualTo(cuentaEntity.IsDeleted));
    }
}
