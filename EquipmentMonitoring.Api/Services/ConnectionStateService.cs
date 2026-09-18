using EquipmentMonitoring.Core.Enums;

namespace EquipmentMonitoring.Api.Services;

public class ConnectionStateService
{
    public ConnectionState State { get; private set; }
        = ConnectionState.Disconnected;
    public DateTime? LastReceivedAt { get; private set; }

    public void SetState(ConnectionState state)
    {
        State = state;
    }
    public void MarkReceived()
    {
        LastReceivedAt = DateTime.Now;
    }
}
