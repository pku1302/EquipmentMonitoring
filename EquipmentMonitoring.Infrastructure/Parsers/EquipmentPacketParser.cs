using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Infrastructure.Parsers;

public class EquipmentPacketParser
{
    public bool TryParse(
    string packet,
    out EquipmentData? data)
    {
        data = null;

        var parts = packet.Split('|');

        if (parts.Length != 6)
            return false;

        if (!double.TryParse(parts[2], out var temperature))
            return false;

        if (!double.TryParse(parts[3], out var pressure))
            return false;

        if (!int.TryParse(parts[4], out var motorRpm))
            return false;

        if (!int.TryParse(parts[5], out var productionCount))
            return false;

        data = new EquipmentData
        {
            EquipmentId = parts[0],
            Status = parts[1],
            Temperature = temperature,
            Pressure = pressure,
            MotorRpm = motorRpm,
            ProductionCount = productionCount,
            Timestamp = DateTime.Now
        };

        return true;
    }
}
