using EquipmentMonitoring.Models;

namespace EquipmentMonitoring.Services;

public class EquipmentStateService
{
    public Equipment CurrentEquipment { get; } = new()
    {
        EquipmentId = "EQ01",
        Name = "Assembly MAchine",
        Status = "STOP"
    };

    public event Action<Equipment>? EquipmentUpdated;
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
