using EquipmentMonitoring.Core.Enums;
using EquipmentMonitoring.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;


namespace EquipmentMonitoring.Services;
public class MonitoringSignalRService
{
    private HubConnection? _connection;
    public event Action<EquipmentData>? EquipmentUpdated;
    public event Action<Alarm>? AlarmRaised;
    public event Action<Alarm>? AlarmCleared;
    public event Action<ConnectionState>? ConnectionStateChanged;

    public async Task ConnectAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7257/monitoringHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<EquipmentData>(
            "EquipmentUpdated",
            data =>
            {
                EquipmentUpdated?.Invoke(data);
            });

        _connection.On<ConnectionState>(
            "ConnectionStateChanged",
            state =>
            {
                ConnectionStateChanged?.Invoke(state);
            });

        _connection.On<Alarm>(
            "AlarmRaised",
            alarm =>
            {
                AlarmRaised?.Invoke(alarm);
            });

        _connection.On<Alarm>(
            "AlarmCleared",
            alarm =>
            {
                AlarmCleared?.Invoke(alarm);
            });

        await _connection.StartAsync();
    }
    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }

}
