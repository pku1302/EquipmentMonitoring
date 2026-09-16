using EquipmentMonitoring.Api.Hubs;
using EquipmentMonitoring.Api.Services;
using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Infrastructure.Parsers;
using Microsoft.AspNetCore.SignalR;

namespace EquipmentMonitoring.Api.BackgroundServices;

public class EquipmentWorker : BackgroundService
{
    private readonly IEquipmentCommunication _communication;
    private readonly EquipmentPacketParser _parser;
    private readonly EquipmentStateService _stateService;
    private readonly AlarmService _alarmService;
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly ILogger<EquipmentWorker> _logger;

    public EquipmentWorker(
        IEquipmentCommunication communication,
        EquipmentPacketParser parser,
        EquipmentStateService stateService,
        AlarmService alarmService,
        IHubContext<MonitoringHub> hubContext,
        ILogger<EquipmentWorker> logger)
    {
        _communication = communication;
        _parser = parser;
        _stateService = stateService;
        _alarmService = alarmService;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation(
                "Connecting to simulator...");

            await _communication.ConnectAsync(
                "127.0.0.1",
                5000,
                stoppingToken);

            _logger.LogInformation(
                "Connected to simulator.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var packet =
                    await _communication.ReceiveAsync(
                        stoppingToken);

                if (packet == null)
                {
                    _logger.LogWarning(
                        "Connection closed.");

                    break;
                }

                if (_parser.TryParse(packet, out var data))
                {
                    _stateService.Update(data!);

                    await _hubContext.Clients.All.SendAsync(
                        "EquipmentUpdated",
                        data,
                        stoppingToken);

                    await _alarmService.CheckAsync(
                        data!,
                        stoppingToken);

                    _logger.LogInformation(
                        "Equipment={EquipmentId}, Temp={Temperature}, Pressure={Pressure}, RPM={Rpm}, Production={Production}",
                        data!.EquipmentId,
                        data.Temperature,
                        data.Pressure,
                        data.MotorRpm,
                        data.ProductionCount);
                }
                else
                {
                    _logger.LogWarning(
                        "Invalid packet: {Packet}",
                        packet);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 서버 종료
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Equipment communication failed");
        }
    }
}
