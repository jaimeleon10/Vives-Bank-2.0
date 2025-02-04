using Quartz;

namespace Banco_VivesBank.Movimientos.Scheduler;

public class DomiciliacionJob : IJob
{
    private readonly ILogger<DomiciliacionJob> _logger;
    private readonly DomiciliacionScheduler _scheduler;

    public DomiciliacionJob(ILogger<DomiciliacionJob> logger, DomiciliacionScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Ejecutando Job de procesamiento de domiciliaciones");

        try
        {
            await _scheduler.ProcesarDomiciliacionesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar el Job de domiciliaciones");
        }
    }
}