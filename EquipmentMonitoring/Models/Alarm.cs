using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Models;

public class Alarm
{
    public int Id { get; set; }
    
    public string EquipmentId { get; set; }
        = string.Empty;
    
    public string AlarmCode { get; set; }
        = string.Empty;
    
    public string AlarmMessage { get; set; }
        = string.Empty;
    
    public DateTime OccurredAt { get; set; }
    
    public DateTime? ClearedAt { get; set; }

}
