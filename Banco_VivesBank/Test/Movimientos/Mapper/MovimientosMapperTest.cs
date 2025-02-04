using Banco_VivesBank.Movimientos.Dto;
using Banco_VivesBank.Movimientos.Mapper;
using Banco_VivesBank.Movimientos.Models;

namespace Test.Movimientos.Mapper;

[TestFixture]
public class MovimientoMapperTests
{
    [Test]
    public void ToResponseFromModel()
    {
        var movimiento = new Movimiento
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now
        };

        var domiciliacionResponse = new DomiciliacionResponse
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = "Acreedor",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 1000.50,
            Periodicidad = "Mensual",
            Activa = true,
            FechaInicio = DateTime.UtcNow.ToString("dd/MM/yyyy"),
            UltimaEjecuccion = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };

        var ingresoNominaResponse = new IngresoNominaResponse { };
        var pagoConTarjetaResponse = new PagoConTarjetaResponse { };
        var transferenciaResponse = new TransferenciaResponse { };
        
        var result = movimiento.ToResponseFromModel(domiciliacionResponse, ingresoNominaResponse, pagoConTarjetaResponse, transferenciaResponse);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(movimiento.ClienteGuid));
        Assert.That(result.Domiciliacion, Is.Not.Null);
        Assert.That(result.Domiciliacion.Guid, Is.EqualTo(domiciliacionResponse.Guid));
        Assert.That(result.Domiciliacion.ClienteGuid, Is.EqualTo(domiciliacionResponse.ClienteGuid));
        Assert.That(result.Domiciliacion.Acreedor, Is.EqualTo(domiciliacionResponse.Acreedor));
        Assert.That(result.Domiciliacion.IbanEmpresa, Is.EqualTo(domiciliacionResponse.IbanEmpresa));
        Assert.That(result.Domiciliacion.IbanCliente, Is.EqualTo(domiciliacionResponse.IbanCliente));
        Assert.That(result.Domiciliacion.Importe, Is.EqualTo(domiciliacionResponse.Importe));
        Assert.That(result.Domiciliacion.Periodicidad, Is.EqualTo(domiciliacionResponse.Periodicidad));
        Assert.That(result.Domiciliacion.Activa, Is.EqualTo(domiciliacionResponse.Activa));
        Assert.That(result.Domiciliacion.FechaInicio, Is.EqualTo(domiciliacionResponse.FechaInicio));
        Assert.That(result.Domiciliacion.UltimaEjecuccion, Is.EqualTo(domiciliacionResponse.UltimaEjecuccion));
        Assert.That(result.IngresoNomina, Is.EqualTo(ingresoNominaResponse));
        Assert.That(result.PagoConTarjeta, Is.EqualTo(pagoConTarjetaResponse));
        Assert.That(result.Transferencia, Is.EqualTo(transferenciaResponse));
    }

    [Test]
    public void ToResponseFromModel_ConResponseNull()
    {
        var movimiento = new Movimiento
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now
        };

        var domiciliacionResponse = new DomiciliacionResponse
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = "Acreedor",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 1000.50,
            Periodicidad = "Mensual",
            Activa = true,
            FechaInicio = DateTime.UtcNow.ToString("dd/MM/yyyy"),
            UltimaEjecuccion = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };
        
        var result = movimiento.ToResponseFromModel(domiciliacionResponse, null, null, null);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(movimiento.ClienteGuid));
        Assert.That(result.Domiciliacion, Is.Not.Null);
        Assert.That(result.Domiciliacion.Guid, Is.EqualTo(domiciliacionResponse.Guid));
        Assert.That(result.Domiciliacion.ClienteGuid, Is.EqualTo(domiciliacionResponse.ClienteGuid));
        Assert.That(result.Domiciliacion.Acreedor, Is.EqualTo(domiciliacionResponse.Acreedor));
        Assert.That(result.Domiciliacion.IbanEmpresa, Is.EqualTo(domiciliacionResponse.IbanEmpresa));
        Assert.That(result.Domiciliacion.IbanCliente, Is.EqualTo(domiciliacionResponse.IbanCliente));
        Assert.That(result.Domiciliacion.Importe, Is.EqualTo(domiciliacionResponse.Importe));
        Assert.That(result.Domiciliacion.Periodicidad, Is.EqualTo(domiciliacionResponse.Periodicidad));
        Assert.That(result.Domiciliacion.Activa, Is.EqualTo(domiciliacionResponse.Activa));
        Assert.That(result.Domiciliacion.FechaInicio, Is.EqualTo(domiciliacionResponse.FechaInicio));
        Assert.That(result.Domiciliacion.UltimaEjecuccion, Is.EqualTo(domiciliacionResponse.UltimaEjecuccion));
        Assert.That(result.IngresoNomina, Is.Null);
        Assert.That(result.PagoConTarjeta, Is.Null);
        Assert.That(result.Transferencia, Is.Null);
    }

    [Test]
    public void ToResponseFromModel_DomiciliacionSinTodosLosCampos()
    {
        var movimiento = new Movimiento
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now
        };

        var domiciliacionResponse = new DomiciliacionResponse
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = "Acreedor",
            Importe = 1000.50,
            Periodicidad = "Mensual",
            Activa = false,
            FechaInicio = DateTime.UtcNow.ToString("dd/MM/yyyy"),
            UltimaEjecuccion = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };
        
        var result = movimiento.ToResponseFromModel(domiciliacionResponse, null, null, null);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(result.ClienteGuid, Is.EqualTo(movimiento.ClienteGuid));
        Assert.That(result.Domiciliacion, Is.Not.Null);
        Assert.That(result.Domiciliacion.Guid, Is.EqualTo(domiciliacionResponse.Guid));
        Assert.That(result.Domiciliacion.ClienteGuid, Is.EqualTo(domiciliacionResponse.ClienteGuid));
        Assert.That(result.Domiciliacion.Acreedor, Is.EqualTo(domiciliacionResponse.Acreedor));
        Assert.That(result.Domiciliacion.Importe, Is.EqualTo(domiciliacionResponse.Importe));
        Assert.That(result.Domiciliacion.Periodicidad, Is.EqualTo(domiciliacionResponse.Periodicidad));
        Assert.That(result.Domiciliacion.Activa, Is.EqualTo(domiciliacionResponse.Activa));
        Assert.That(result.Domiciliacion.FechaInicio, Is.EqualTo(domiciliacionResponse.FechaInicio));
        Assert.That(result.Domiciliacion.UltimaEjecuccion, Is.EqualTo(domiciliacionResponse.UltimaEjecuccion));
    }

    [Test]
    public void ToResponseFromModel_SinFormatoCreateAt()
    {
        var movimiento = new Movimiento
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.MinValue 
        };

        var domiciliacionResponse = new DomiciliacionResponse
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Acreedor = "Acreedor",
            IbanEmpresa = "ES1234567890123456789012",
            IbanCliente = "ES9876543210987654321098",
            Importe = 1000.50,
            Periodicidad = "Mensual",
            Activa = true,
        };

        var result = movimiento.ToResponseFromModel(domiciliacionResponse, null, null, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreatedAt, Is.EqualTo(movimiento.CreatedAt.ToString("dd/MM/yyyy H:mm:ss")));
    }
}