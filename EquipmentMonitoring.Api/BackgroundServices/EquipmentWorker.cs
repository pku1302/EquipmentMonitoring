using EquipmentMonitoring.Api.Hubs;
using EquipmentMonitoring.Api.Services;
using EquipmentMonitoring.Core.Enums;
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
    private readonly ConnectionStateService _connectionState;

    public EquipmentWorker(
        IEquipmentCommunication communication,
        EquipmentPacketParser parser,
        EquipmentStateService stateService,
        ConnectionStateService connectionState,
        AlarmService alarmService,
        IHubContext<MonitoringHub> hubContext,
        ILogger<EquipmentWorker> logger)
    {
        _communication = communication;
        _parser = parser;
        _stateService = stateService;
        _connectionState = connectionState;
        _alarmService = alarmService;
        _hubContext = hubContext;
        _logger = logger;

        _communication.Connected += OnConnected;
        _communication.Disconnected += OnDisconnected;
        _communication.Reconnecting += OnReconnecting;
        _communication.ReconnectFailed += OnReconnectFailed;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_communication.IsConnected)
                {
                    _connectionState.SetState(
                        ConnectionState.Connecting);

                    _logger.LogInformation(
                        "Connecting to simulator...");

                    await _communication.ConnectAsync(
                        "127.0.0.1",
                        5000,
                        stoppingToken);

                    _logger.LogInformation(
                        "Connected to simulator.");
                }

                var packet =
                    await _communication.ReceiveAsync(
                        stoppingToken);

                if (packet == null)
                {
                    await TryReconnectAsync(
                        stoppingToken);

                    continue;
                }

                _connectionState.MarkReceived();

                if (!_parser.TryParse(
                        packet,
                        out var data))
                {
                    _logger.LogWarning(
                        "Invalid packet: {Packet}",
                        packet);

                    continue;
                }

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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Equipment communication failed");

                await TryReconnectAsync(
                    stoppingToken);
            }
        }
    }

    private async Task TryReconnectAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var success =
                await _communication.ReconnectAsync(
                    cancellationToken);

            if (!success)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 서버 종료 중
        }
    }

    private void OnConnected()
    {
        _connectionState.SetState(
            ConnectionState.Connected);

        _ = _hubContext.Clients.All.SendAsync(
            "ConnectionStateChanged",
            ConnectionState.Connected);
    }

    private void OnDisconnected()
    {
        _connectionState.SetState(
            ConnectionState.Disconnected);

        _ = _hubContext.Clients.All.SendAsync(
            "ConnectionStateChanged",
            ConnectionState.Disconnected);
    }

    private void OnReconnecting(int attempt)
    {
        _connectionState.SetState(
            ConnectionState.Reconnecting);

        _ = _hubContext.Clients.All.SendAsync(
            "ConnectionStateChanged",
            ConnectionState.Reconnecting);
    }
    private void OnReconnectFailed()
    {
        _connectionState.SetState(
            ConnectionState.Failed);

        _ = _hubContext.Clients.All.SendAsync(
            "ConnectionStateChanged",
            ConnectionState.Failed);
    }
}
