using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Core.Models;

public class EquipmentData
{
    public string EquipmentId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public double Temperature { get; set; }

    public double Pressure { get; set; }

    public int MotorRpm { get; set; }

    public int ProductionCount { get; set; }

    public DateTime Timestamp { get; set; }

}
