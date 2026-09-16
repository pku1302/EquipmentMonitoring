using EquipmentMonitoring.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;


namespace EquipmentMonitoring.Services;
public class MonitoringSignalRService
{
    private HubConnection? _connection;

    public event Action<EquipmentData>? EquipmentUpdated;

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
