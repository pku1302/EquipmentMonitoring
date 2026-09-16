using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Core.Interfaces;
public interface IPlcClient
{
    bool IsConnected { get; }

    Task ConectAsync(
        CancellationToken cancellationToken = default);

    Task DisconnectAsync();

}
