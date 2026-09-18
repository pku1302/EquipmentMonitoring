using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Core.Enums;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}
