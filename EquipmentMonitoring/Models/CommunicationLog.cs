using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Models;

public class CommunicationLog
{
    public DateTime Timestamp { get; set; }

    public string Level { get; set; }
        = string.Empty;

    public string Source { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;
}
