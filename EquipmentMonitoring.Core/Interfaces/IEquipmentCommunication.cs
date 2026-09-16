using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Core.Interfaces;

public interface IEquipmentCommunication
{
    Task ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default);

    Task<string?> ReceiveAsync(
        CancellationToken cancellationToken = default);
}
