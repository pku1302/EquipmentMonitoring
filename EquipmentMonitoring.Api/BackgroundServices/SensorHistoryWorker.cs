using EquipmentMonitoring.Api.Services;
using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Api.BackgroundServices;

public class SensorHistoryWorker : BackgroundService
{
    private readonly EquipmentStateService _stateService;
    private readonly ISensorRepository _sensorRepository;
    private readonly ILogger<SensorHistoryWorker> _logger;

    public SensorHistoryWorker(
        EquipmentStateService stateService,
        ISensorRepository sensorRepository,
        ILogger<SensorHistoryWorker> logger)
    {
        _stateService = stateService;
        _sensorRepository = sensorRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var equipments = _stateService.GetAll();

                foreach (var equipment in equipments)
                {
                    var history = new EquipmentData
                    {
                        EquipmentId = equipment.EquipmentId,
                        Temperature = equipment.Temperature,
                        Pressure = equipment.Pressure,
                        MotorRpm = equipment.MotorRpm,
                        ProductionCount = equipment.ProductionCount,
                        Timestamp = DateTime.UtcNow
                    };

                    await _sensorRepository.InsertAsync(
                        history,
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Sensor history save failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

}
