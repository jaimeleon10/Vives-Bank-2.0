using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Utils.Validators;

public class CardLimitValidators
{
    
    private readonly ILogger<CardLimitValidators> _logger;
    
    public CardLimitValidators(ILogger<CardLimitValidators> logger)
    {
        _logger = logger;
    }
    
    public bool ValidarLimite(TarjetaRequest dto)
    {
        var diario = dto.LimiteDiario;
        var tripleDiario = diario * 3;
        var semanal = dto.LimiteSemanal;
        var tripleSemanal = semanal * 3;
        var mensual = dto.LimiteMensual;

        if (diario <= 0)
        {
            _logger.LogWarning("El limite diario debe ser superior a 0");
            return false;
        }
        if (semanal < tripleDiario)
        {
            _logger.LogWarning("El limite semanal debe ser igual o superior a 3 veces el diario");
            return false;
        }
        if (mensual < tripleSemanal)
        {
            _logger.LogWarning("El limite mensual debe ser igual o superior a 3 veces el semanal");
            return false;
        }
        return true;
    }
}