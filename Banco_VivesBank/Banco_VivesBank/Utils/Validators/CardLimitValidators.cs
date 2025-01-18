using Banco_VivesBank.Producto.Tarjeta.Dto;

namespace Banco_VivesBank.Utils.Validators;

public class CardLimitValidators
{
    
    private static readonly ILogger<CardLimitValidators> _logger;
    
    public bool ValidarLimite(TarjetaRequestDto dto)
    {
        var diario = dto.LimiteDiario;
        var semanal = dto.LimiteSemanal;
        var mensual = dto.LimiteMensual;

        if (diario <= 0)
        {
            _logger.LogWarning("El limite diario debe ser superior a 0");
            return false;
        }
        if (semanal <= (diario * 3))
        {
            _logger.LogWarning("El limite semanal debe ser igual o superior a 3 veces el diario");
            return false;
        }
        if (mensual <= (semanal * 3))
        {
            _logger.LogWarning("El limite mensual debe ser igual o superior a 3 veces el semanal");
            return false;
        }
        return true;
    }
}