using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Core.Interfaces;

public interface IEquipmentCommunication
{
    bool IsConnected { get; }

    event Action? Connected;
    event Action? Disconnected;
    event Action<int>? Reconnecting;
    event Action? ReconnectFailed;

    Task ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default);

    Task<string?> ReceiveAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ReconnectAsync(
        CancellationToken cancellationToken = default);

    Task DisconnectAsync();
}
