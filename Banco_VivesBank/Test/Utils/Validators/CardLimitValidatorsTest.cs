﻿using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Utils.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace Test.Utils.Validators;

public class CardLimitValidatorsTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private CardLimitValidators _cardLimitValidators;
    private readonly ILogger<CardLimitValidators> _logger;

    [SetUp]
    public async Task Setup()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();

        await _postgreSqlContainer.StartAsync();

        _cardLimitValidators = new CardLimitValidators(NullLogger<CardLimitValidators>.Instance);

    }
    
    [TearDown]
    public async Task Teardown()
    {

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }

    [Test]
    public async Task ValidarLimiteOk()
    {
        
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };

        var res = _cardLimitValidators.ValidarLimite(tarjetaRequest);
        
        Assert.That(res, Is.True);
    }
    
    [Test]
    public async Task ValidarLimiteDiarioExcedido()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 0,
            LimiteSemanal = 3000,
            LimiteMensual = 9000
        };

        var res = _cardLimitValidators.ValidarLimite(tarjetaRequest);
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public async Task ValidarLimiteSemanalExcedido()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 2000,
            LimiteMensual = 9000
        };

        var res = _cardLimitValidators.ValidarLimite(tarjetaRequest);
        
        Assert.That(res, Is.False);
    }
    
    [Test]
    public async Task ValidarLimiteMensualExcedido()
    {
        var tarjetaRequest = new TarjetaRequest
        {
            Pin = "1234",
            LimiteDiario = 1000,
            LimiteSemanal = 3000,
            LimiteMensual = 8000
        };

        var res = _cardLimitValidators.ValidarLimite(tarjetaRequest);
        
        Assert.That(res, Is.False);
    }
    
}