using EquipmentMonitoring.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace EquipmentMonitoring.Api.Hubs;

public class MonitoringHub : Hub
{
    private readonly ConnectionStateService _connectionStateService;

    public MonitoringHub(
        ConnectionStateService connectionStateService)
    {
        _connectionStateService = connectionStateService;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync(
            "ConnectionStateChanged",
            _connectionStateService.State);

        await base.OnConnectedAsync();
    }
}
