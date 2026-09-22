using EquipmentMonitoring.Core.Enums;
using EquipmentMonitoring.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentMonitoring.Models;

public class AlarmItemViewModel
{
    private readonly Alarm _alarm;
    public AlarmItemViewModel(Alarm alarm)
    {
        _alarm = alarm;
    }

    public long Id => _alarm.Id;

    public string EquipmentId => _alarm.EquipmentId;

    public AlarmCode AlarmCode => _alarm.AlarmCode;

    public string AlarmMessage => _alarm.AlarmMessage;

    public AlarmSeverity Severity => _alarm.Severity;

    public bool IsActive => _alarm.IsActive;

    // 원본 UTC
    public DateTime OccurredAtUtc => _alarm.OccurredAt;

    public DateTime? ClearedAtUtc => _alarm.ClearedAt;

    // UI 표시용 LocalTime
    public DateTime OccurredAtLocal =>
        _alarm.OccurredAt.ToLocalTime();

    public DateTime? ClearedAtLocal =>
        _alarm.ClearedAt?.ToLocalTime();

    // UI 바인딩용 문자열
    public string OccurredAtText =>
        OccurredAtLocal.ToString("yyyy-MM-dd HH:mm:ss");

    public string ClearedAtText =>
        ClearedAtLocal?.ToString("yyyy-MM-dd HH:mm:ss")
        ?? "-";
}
