using EquipmentMonitoring.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Core.Models;

public class Alarm
{
    public long Id { get; set; }

    public string EquipmentId { get; set; } = string.Empty;

    public AlarmCode AlarmCode { get; set; }

    public string AlarmMessage { get; set; } = string.Empty;

    public AlarmSeverity Severity { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime? ClearedAt { get; set; }

    public bool IsActive { get; set; }
}
