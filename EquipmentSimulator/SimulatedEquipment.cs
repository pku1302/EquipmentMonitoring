using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentSimulator;

public class SimulatedEquipment
{
    public string EquipmentId { get; set; } = "";
    public string Status { get; set; } = "RUN";
    public int ProductionCount { get; set; }
    public double Temperature { get; set; }
    public double Pressure { get; set; }
    public int MotorRpm { get; set; }
}
