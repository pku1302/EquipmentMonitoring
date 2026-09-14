using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentMonitoring.Models;
using EquipmentMonitoring.Parsers;
using EquipmentMonitoring.Repositories;

namespace EquipmentMonitoring.Services;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}

public partial class EquipmentService : ObservableObject
{
    [ObservableProperty]
    private ConnectionState _connectionState
        = ConnectionState.Disconnected;

    private readonly TcpCommunicationService _tcpService;
    private readonly EquipmentStateService _stateService;
    private readonly EquipmentPacketParser _packetParser;
    private readonly LogService _logService;
    private readonly SensorRepository _sensorRepository;
    private readonly AlarmRepository _alarmRepository;
    private bool _temperatureAlarmActive;
    private const double HighTemperatureLimit 
        = 80.0;

    public Equipment CurrentEquipment
        => _stateService.CurrentEquipment;

    public event Action? StateChanged;

    public EquipmentService(
        TcpCommunicationService tcpService,
        EquipmentStateService stateService,
        EquipmentPacketParser packetParser,
        SensorRepository sensorRepository,
        AlarmRepository alarmRepository,
        LogService logService)
    {
        _tcpService = tcpService;
        _stateService = stateService;
        _packetParser = packetParser;
        _logService = logService;
        _sensorRepository = sensorRepository;
        _alarmRepository = alarmRepository;

        _tcpService.Connected += OnConnected;
        _tcpService.Disconnected += OnDisconnected;
        _tcpService.Reconnecting += OnReconnecting;
        _tcpService.DataReceived += OnDataReceived;
        _tcpService.ReconnectFailed += OnReconnectFailed;
    }

    public async Task ConnectAsync()
    {
        try
        {
            ConnectionState = 
                ConnectionState.Connected;

            await _logService.AddAsync(
                "INFO",
                "TCP",
                "Connecting to 127.0.0.1:5000");

            await _tcpService.ConnectAsync(
                "127.0.0.1",
                5000);
        }
        catch (Exception ex)
        {
            ConnectionState =
                ConnectionState.Failed;

            await _logService.AddAsync(
                "ERROR",
                "TCP",
                $"Connection failed: {ex.Message}");
        }
    }
    private async void OnDataReceived(
        string packet)
    {
        try
        {
            await _logService.AddAsync(
                "RX",
                "TCP",
                packet);

            if (!_packetParser.TryParse(
                    packet,
                    out EquipmentData? data))
            {
                await _logService.AddAsync(
                    "WARNING",
                    "PARSER",
                    $"Invalid packet: {packet}");

                return;
            }

            if (data == null)
                return;

            _stateService.Update(data);

            await SaveSensorHistoryAsync(data);

            await CheckAlarmAsync(data);


        }
        catch (Exception ex)
        {
            await _logService.AddAsync(
                "ERROR",
                "EQUIPMENT",
                $"Packet processing failed: {ex.Message}");
        }
    }

    private async Task SaveSensorHistoryAsync(
        EquipmentData data)
    {
        try
        {
            var history = new SensorHistory
            {
                EquipmentId = data.EquipmentId,
                Temperature = data.Temperature,
                Pressure = data.Pressure,
                MotorRpm = data.MotorRpm,
                ProductionCount = data.ProductionCount,
                CreatedAt = DateTime.Now
            };

            await _sensorRepository.InsertAsync(history);
        }
        catch (Exception ex)
        {
            await _logService.AddAsync(
                "ERROR",
                "DATABASE",
                $"Sensor history insert failed: {ex.Message}");
        }
    }

    private async Task SaveAlarmAsync(
        Alarm alarm)
    {
        try
        {
            await _alarmRepository.InsertAsync(alarm);
        }
        catch (Exception ex)
        {
            await _logService.AddAsync(
                "ERROR",
                "DATABASE",
                $"Alarm insert failed: {ex.Message}");
        }
    }

    private async Task CheckAlarmAsync(
        EquipmentData data)
    {
        if (data.Temperature >= HighTemperatureLimit)
        {
            if (_temperatureAlarmActive)
                return;

            _temperatureAlarmActive = true;

            Alarm alarm = new()
            {
                EquipmentId =
                    data.EquipmentId,

                AlarmCode =
                    "TEMP_HIGH",

                AlarmMessage =
                    $"High temperature detected: {data.Temperature:F1} ℃",

                OccurredAt =
                    DateTime.Now
            };

            await SaveAlarmAsync(alarm);
            await _logService.AddAsync(
                "ALARM",
                data.EquipmentId,
                $"TEMP_HIGH - {data.Temperature:F1} ℃");
        }
        else
        {
            _temperatureAlarmActive = false;
        }
    }

    public void Disconnect()
    {
        _tcpService.DisConnect();
    }
    private async void OnConnected()
    {
        ConnectionState = 
            ConnectionState.Connected;

        await _logService.AddAsync(
            "INFO",
            "TCP",
            "Connected");

        StateChanged?.Invoke();
    }
    private async void OnDisconnected()
    {
        ConnectionState = 
            ConnectionState.Disconnected;

        await _logService.AddAsync(
            "WARNING",
            "TCP",
            "Connection lost");

        StateChanged?.Invoke();
    }
    private async void OnReconnecting(int attempt)
    {
        ConnectionState =
            ConnectionState.Reconnecting;

        await _logService.AddAsync(
            "INFO",
            "TCP",
            $"Reconnect attempt {attempt}");

        StateChanged?.Invoke();
    }

    private async void OnReconnectFailed()
    {
        ConnectionState =
            ConnectionState.Failed;

        await _logService.AddAsync(
            "ERROR",
            "TCP",
            "Reconnect failed after 3 attempts");
    }
}
