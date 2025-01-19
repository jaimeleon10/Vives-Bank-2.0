using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Base.Dto;
using Banco_VivesBank.Producto.Cuenta.Mappers;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using NUnit.Framework;

namespace Banco_VivesBank.Test.Producto.Cuenta;

[TestFixture]
public class CuentaMapperTest
{
    [Test]
    public void ToModelFromEntity()
    {
        var cuentaEntity = new CuentaEntity
        {
            Guid = "entity-guid",
            Iban = "ES7620770024003102575766",
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 2,
            ProductoId = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var cuentaModel = CuentaMapper.ToModelFromEntity(cuentaEntity);
        
        Assert.Multiple(() =>
        {
            Assert.That(cuentaModel.Guid, Is.EqualTo(cuentaEntity.Guid));
            Assert.That(cuentaModel.Iban, Is.EqualTo(cuentaEntity.Iban));
            Assert.That(cuentaModel.Saldo, Is.EqualTo(cuentaEntity.Saldo));
            Assert.That(cuentaModel.TarjetaId, Is.EqualTo(cuentaEntity.TarjetaId));
            Assert.That(cuentaModel.ClienteId, Is.EqualTo(cuentaEntity.ClienteId));
            Assert.That(cuentaModel.ProductoId, Is.EqualTo(cuentaEntity.ProductoId));
            Assert.That(cuentaModel.CreatedAt, Is.EqualTo(cuentaEntity.CreatedAt));
            Assert.That(cuentaModel.UpdatedAt, Is.EqualTo(cuentaEntity.UpdatedAt));
            Assert.That(cuentaModel.IsDeleted, Is.EqualTo(cuentaEntity.IsDeleted));
        });
    }
    
    [Test]
    public void ToEntityFromModel()
    {
        var cuentaModel = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Guid = "model-guid",
            Iban = "ES7620770024003102575766",
            Saldo = 1000,
            TarjetaId = 1,
            ClienteId = 2,
            ProductoId = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        var cuentaEntity = CuentaMapper.ToEntityFromModel(cuentaModel);
        
        Assert.Multiple(() =>
        {
            Assert.That(cuentaEntity.Guid, Is.EqualTo(cuentaModel.Guid));
            Assert.That(cuentaEntity.Iban, Is.EqualTo(cuentaModel.Iban));
            Assert.That(cuentaEntity.Saldo, Is.EqualTo(cuentaModel.Saldo));
            Assert.That(cuentaEntity.TarjetaId, Is.EqualTo(cuentaModel.TarjetaId));
            Assert.That(cuentaEntity.ClienteId, Is.EqualTo(cuentaModel.ClienteId));
            Assert.That(cuentaEntity.ProductoId, Is.EqualTo(cuentaModel.ProductoId));
            Assert.That(cuentaEntity.CreatedAt, Is.EqualTo(cuentaModel.CreatedAt));
            Assert.That(cuentaEntity.UpdatedAt, Is.EqualTo(cuentaModel.UpdatedAt));
            Assert.That(cuentaEntity.IsDeleted, Is.EqualTo(cuentaModel.IsDeleted));
        });
    }
    
    [Test]
    public void ToResponseFromModel()
    {
        var cuentaModel = new Banco_VivesBank.Producto.Cuenta.Models.Cuenta
        {
            Guid = "model-guid",
            Iban = "ES7620770024003102575766",
            Saldo = 1000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        string tarjetaGuid = "tarjeta-guid";
        string clienteGuid = "cliente-guid";
        string productoGuid = "producto-guid";
        
        var cuentaResponse = CuentaMapper.ToResponseFromModel(cuentaModel, tarjetaGuid, clienteGuid, productoGuid);
        
        Assert.Multiple(() =>
        {
            Assert.That(cuentaResponse.Guid, Is.EqualTo(cuentaModel.Guid));
            Assert.That(cuentaResponse.Iban, Is.EqualTo(cuentaModel.Iban));
            Assert.That(cuentaResponse.Saldo, Is.EqualTo(cuentaModel.Saldo));
            Assert.That(cuentaResponse.TarjetaGuid, Is.EqualTo(tarjetaGuid));
            Assert.That(cuentaResponse.ClienteGuid, Is.EqualTo(clienteGuid));
            Assert.That(cuentaResponse.ProductoGuid, Is.EqualTo(productoGuid));
            Assert.That(cuentaResponse.CreatedAt, Is.EqualTo(cuentaModel.CreatedAt));
            Assert.That(cuentaResponse.UpdatedAt, Is.EqualTo(cuentaModel.UpdatedAt));
            Assert.That(cuentaResponse.IsDeleted, Is.EqualTo(cuentaModel.IsDeleted));
        });
    }
    
    [Test]
    public void ToResponseFromEntity()
    {
        var cuentaEntity = new CuentaEntity
        {
            Guid = "entity-guid",
            Iban = "ES7620770024003102575766",
            Saldo = 1000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var tarjetaResponse = new TarjetaResponseDto { Guid = "tarjeta-guid" };
        var clienteResponse = new ClienteResponse { Guid = "cliente-guid" };
        var productoResponse = new BaseResponse { Guid = "producto-guid" };
        
        var cuentaResponse = CuentaMapper.ToResponseFromEntity(cuentaEntity, tarjetaResponse, clienteResponse, productoResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(cuentaResponse.Guid, Is.EqualTo(cuentaEntity.Guid));
            Assert.That(cuentaResponse.Iban, Is.EqualTo(cuentaEntity.Iban));
            Assert.That(cuentaResponse.Saldo, Is.EqualTo(cuentaEntity.Saldo));
            Assert.That(cuentaResponse.TarjetaGuid, Is.EqualTo(tarjetaResponse.Guid));
            Assert.That(cuentaResponse.ClienteGuid, Is.EqualTo(clienteResponse.Guid));
            Assert.That(cuentaResponse.ProductoGuid, Is.EqualTo(productoResponse.Guid));
            Assert.That(cuentaResponse.CreatedAt, Is.EqualTo(cuentaEntity.CreatedAt));
            Assert.That(cuentaResponse.UpdatedAt, Is.EqualTo(cuentaEntity.UpdatedAt));
            Assert.That(cuentaResponse.IsDeleted, Is.EqualTo(cuentaEntity.IsDeleted));
        });
    }
}