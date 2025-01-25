using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Mappers;

namespace Test.Producto.Tarjeta.Mappers;

public class TarjetaMapperTest
{
    [Test]
    public void ToEntityFromModel()
    {
        // Arrange
        var tarjeta = new Banco_VivesBank.Producto.Tarjeta.Models.Tarjeta
        {
            Id = 1,
            Guid = "Test Guid",
            Numero = "1234567890123456",
            FechaVencimiento = "12_29",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0,
            CreatedAt =     DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Act
        var model = TarjetaMapper.ToEntityFromModel(tarjeta);

        // Assert
        Assert.That(model.Id, Is.EqualTo(tarjeta.Id));
        Assert.That(model.Guid, Is.EqualTo(tarjeta.Guid));
        Assert.That(model.Numero, Is.EqualTo(tarjeta.Numero));
        Assert.That(model.FechaVencimiento, Is.EqualTo(tarjeta.FechaVencimiento));
        Assert.That(model.Cvv, Is.EqualTo(tarjeta.Cvv));
        Assert.That(model.Pin, Is.EqualTo(tarjeta.Pin));
        Assert.That(model.LimiteDiario, Is.EqualTo(tarjeta.LimiteDiario));
        Assert.That(model.LimiteSemanal, Is.EqualTo(tarjeta.LimiteSemanal));
        Assert.That(model.LimiteMensual, Is.EqualTo(tarjeta.LimiteMensual));
        Assert.That(model.CreatedAt, Is.EqualTo(tarjeta.CreatedAt));
        Assert.That(model.UpdatedAt, Is.EqualTo(tarjeta.UpdatedAt));
        Assert.That(model.IsDeleted, Is.EqualTo(tarjeta.IsDeleted));
    }

    [Test]
    public void ToModelFromRequest()
    {
        // Arrange
        var tarjetaRequest = new TarjetaRequest()
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0
        };

        // Act
        var model = TarjetaMapper.ToModelFromRequest(tarjetaRequest);

        // Assert

        Assert.That(model.Pin, Is.EqualTo(tarjetaRequest.Pin));
        Assert.That(model.LimiteDiario, Is.EqualTo(tarjetaRequest.LimiteDiario));
        Assert.That(model.LimiteSemanal, Is.EqualTo(tarjetaRequest.LimiteSemanal));
        Assert.That(model.LimiteMensual, Is.EqualTo(tarjetaRequest.LimiteMensual));
    }

    [Test]
    public void ToModelFromEntity()
    {
        // Arrange
        var tarjetaEntity = new TarjetaEntity()
        {
            Id = 1,
            Guid = "Test Guid",
            Numero = "1234567890123456",
            FechaVencimiento = "12_29",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Act
        var model = TarjetaMapper.ToModelFromEntity(tarjetaEntity);

        // Assert
        Assert.That(model.Id, Is.EqualTo(tarjetaEntity.Id));
        Assert.That(model.Guid, Is.EqualTo(tarjetaEntity.Guid));
        Assert.That(model.Numero, Is.EqualTo(tarjetaEntity.Numero));
        Assert.That(model.FechaVencimiento, Is.EqualTo(tarjetaEntity.FechaVencimiento));
        Assert.That(model.Cvv, Is.EqualTo(tarjetaEntity.Cvv));
        Assert.That(model.Pin, Is.EqualTo(tarjetaEntity.Pin));
        Assert.That(model.LimiteDiario, Is.EqualTo(tarjetaEntity.LimiteDiario));
        Assert.That(model.LimiteSemanal, Is.EqualTo(tarjetaEntity.LimiteSemanal));
        Assert.That(model.LimiteMensual, Is.EqualTo(tarjetaEntity.LimiteMensual));
        Assert.That(model.CreatedAt, Is.EqualTo(tarjetaEntity.CreatedAt));
        Assert.That(model.UpdatedAt, Is.EqualTo(tarjetaEntity.UpdatedAt));
        Assert.That(model.IsDeleted, Is.EqualTo(tarjetaEntity.IsDeleted));
    }

    [Test]
    public void ToResponseFromEntity()
    {
        // Arrange
        var tarjetaEntity = new TarjetaEntity()
        {
            Id = 1,
            Guid = "Test Guid",
            Numero = "1234567890123456",
            FechaVencimiento = "12_29",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Act
        var response = TarjetaMapper.ToResponseFromEntity(tarjetaEntity);

        // Assert
        Assert.That(response.Guid, Is.EqualTo(tarjetaEntity.Guid));
        Assert.That(response.Numero, Is.EqualTo(tarjetaEntity.Numero));
        Assert.That(response.FechaVencimiento, Is.EqualTo(tarjetaEntity.FechaVencimiento));
        Assert.That(response.Cvv, Is.EqualTo(tarjetaEntity.Cvv));
        Assert.That(response.Pin, Is.EqualTo(tarjetaEntity.Pin));
        Assert.That(response.LimiteDiario, Is.EqualTo(tarjetaEntity.LimiteDiario));
        Assert.That(response.LimiteSemanal, Is.EqualTo(tarjetaEntity.LimiteSemanal));
        Assert.That(response.LimiteMensual, Is.EqualTo(tarjetaEntity.LimiteMensual));
        Assert.That(response.IsDeleted, Is.EqualTo(tarjetaEntity.IsDeleted));
    }

    [Test]
    public void ToResponseFromModel()
    {
        // Arrange
        var tarjeta = new Banco_VivesBank.Producto.Tarjeta.Models.Tarjeta
        {
            Id = 1,
            Guid = "Test Guid",
            Numero = "1234567890123456",
            FechaVencimiento = "12_29",
            Cvv = "123",
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 0,
            LimiteMensual = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Act
        var response = TarjetaMapper.ToResponseFromModel(tarjeta);

        // Assert
        Assert.That(response.Guid, Is.EqualTo(tarjeta.Guid));
        Assert.That(response.Numero, Is.EqualTo(tarjeta.Numero));
        Assert.That(response.FechaVencimiento, Is.EqualTo(tarjeta.FechaVencimiento));
        Assert.That(response.Cvv, Is.EqualTo(tarjeta.Cvv));
        Assert.That(response.Pin, Is.EqualTo(tarjeta.Pin));
        Assert.That(response.LimiteDiario, Is.EqualTo(tarjeta.LimiteDiario));
        Assert.That(response.LimiteSemanal, Is.EqualTo(tarjeta.LimiteSemanal));
        Assert.That(response.LimiteMensual, Is.EqualTo(tarjeta.LimiteMensual));
      Assert.That(response.IsDeleted, Is.EqualTo(tarjeta.IsDeleted));
    }

    [Test]
    public void ToRResponseList()
    {
        // Arrange
        var tarjetaEntities = new List<TarjetaEntity>()
        {
            new TarjetaEntity()
            {
                Id = 1,
                Guid = "Test Guid 1",
                Numero = "1234567890123456",
                FechaVencimiento = "12_29",
                Cvv = "123",
                Pin = "1234",
                LimiteDiario = 0,
                LimiteSemanal = 0,
                LimiteMensual = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new TarjetaEntity()
            {
                Id = 2,
                Guid = "Test Guid 2",
                Numero = "9876543210987654",
                FechaVencimiento = "03_31",
                Cvv = "321",
                Pin = "5678",
                LimiteDiario = 0,
                LimiteSemanal = 0,
                LimiteMensual = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        };
        
        // Act
        var responses = TarjetaMapper.ToResponseList(tarjetaEntities);

        // Assert
        Assert.That(responses.Count, Is.EqualTo(2));
        Assert.That(responses[0].Guid, Is.EqualTo(tarjetaEntities[0].Guid));
        Assert.That(responses[0].Numero, Is.EqualTo(tarjetaEntities[0].Numero));
        Assert.That(responses[0].FechaVencimiento, Is.EqualTo(tarjetaEntities[0].FechaVencimiento));
        Assert.That(responses[0].Cvv, Is.EqualTo(tarjetaEntities[0].Cvv));
    }
        
}