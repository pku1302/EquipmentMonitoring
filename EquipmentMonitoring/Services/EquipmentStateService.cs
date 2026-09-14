using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentMonitoring.Models;

namespace EquipmentMonitoring.Services;

public partial class EquipmentStateService : ObservableObject
{
    public Equipment CurrentEquipment { get; } = new()
    {
        EquipmentId = "EQ01",
        Name = "Assembly Mqchine",
        Status = "STOP"
    };

    public event Action<Equipment>? EquipmentUpdated;
    public event Action? Connected;
    public event Action? Disconnected;

    [ObservableProperty]
    private ConnectionState _connectionState
        = ConnectionState.Disconnected;

    partial void OnConnectionStateChanged(
        ConnectionState oldValue,
        ConnectionState newValue)
    {
        if (newValue == ConnectionState.Connected &&
            oldValue != ConnectionState.Connected)
        {
            Connected?.Invoke();
        }

        if (newValue == ConnectionState.Disconnected &&
            oldValue != ConnectionState.Disconnected)
        {
            Disconnected?.Invoke();
        }
    }

    public void Update(EquipmentData data)
    {
        CurrentEquipment.EquipmentId = data.EquipmentId;
        CurrentEquipment.Status = data.Status;
        CurrentEquipment.Temperature = data.Temperature;
        CurrentEquipment.Pressure = data.Pressure;
        CurrentEquipment.MotorRpm = data.MotorRpm;
        CurrentEquipment.ProductionCount = data.ProductionCount;

        EquipmentUpdated?.Invoke(CurrentEquipment);
    }
}
