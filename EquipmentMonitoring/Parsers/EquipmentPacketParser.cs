using EquipmentMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EquipmentMonitoring.Parsers;

public class EquipmentPacketParser
{
    public bool TryParse(
        string packet,
        out EquipmentData? data)
    {
        data = null;

        if (string.IsNullOrEmpty(packet))
            return false;

        string[] parts = packet.Split('|');

        if (parts.Length != 6)
            return false;

        if (!double.TryParse(
                parts[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double temperature))
        {
            return false;
        }

        if (!double.TryParse(
                parts[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double pressure))
        {
            return false;
        }

        if (!int.TryParse(
                parts[4],
                out int rpm))
        {
            return false;
        }

        if (!int.TryParse(
                parts[5],
                out int productionCount))
        {
            return false;
        }

        data = new EquipmentData
        {
            EquipmentId = parts[0],
            Status = parts[1],
            Temperature = temperature,
            Pressure = pressure,
            MotorRpm = rpm,
            ProductionCount = productionCount
        };

        return true;
    }
}
