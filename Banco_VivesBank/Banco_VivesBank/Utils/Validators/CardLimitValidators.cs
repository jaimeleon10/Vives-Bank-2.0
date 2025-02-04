using Banco_VivesBank.Producto.Tarjeta.Dto;
using Banco_VivesBank.Producto.Tarjeta.Exceptions;

namespace Banco_VivesBank.Utils.Validators;

public class CardLimitValidators
{
    
    private readonly ILogger<CardLimitValidators> _logger;
    
    public CardLimitValidators(ILogger<CardLimitValidators> logger)
    {
        _logger = logger;
    }
    
    public void ValidarLimite(double limiteDiario, double limiteSemenal, double limiteMensual)
    {
        var diario = limiteDiario;
        var tripleDiario = diario * 3;
        var semanal = limiteSemenal;
        var tripleSemanal = semanal * 3;
        var mensual = limiteMensual;

        if (diario <= 0)
        {
            _logger.LogWarning("El limite diario debe ser superior a 0");
            throw new TarjetaNotFoundException("Error con los limites de gasto de la tarjeta, el diario debe ser superior a 0");
        }
        if (semanal < tripleDiario)
        {
            _logger.LogWarning("El limite semanal debe ser igual o superior a 3 veces el diario");
            throw new TarjetaNotFoundException("Error con los limites de gasto de la tarjeta, el semanal debe ser superior 3 veces al diario");

        }
        if (mensual < tripleSemanal)
        {
            _logger.LogWarning("El limite mensual debe ser igual o superior a 3 veces el semanal");
            throw new TarjetaNotFoundException("Error con los limites de gasto de la tarjeta, el mensual debe ser superior 3 veces al semanal");
        }
        
    }
}