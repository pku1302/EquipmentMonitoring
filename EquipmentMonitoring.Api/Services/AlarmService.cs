using EquipmentMonitoring.Api.Hubs;
using EquipmentMonitoring.Core.Enums;
using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Core.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace EquipmentMonitoring.Api.Services;

public class AlarmService
{
    private const double HighTemperatureLimit = 80.0;

    private readonly IAlarmRepository _alarmRepository;
    private readonly IHubContext<MonitoringHub> _hubContext;
    private readonly ConcurrentDictionary<string, Alarm>
        _activeTemperatureAlarms = new();

    public AlarmService(
        IAlarmRepository alarmRepository,
        IHubContext<MonitoringHub> hubContext)
    {
        _alarmRepository = alarmRepository;
        _hubContext = hubContext;
    }

    public async Task CheckAsync(
        EquipmentData data,
        CancellationToken cancellationToken = default)
    {
        var key = $"{data.EquipmentId}:{AlarmCode.TEMP_HIGH.ToString()}";

        if (data.Temperature >= HighTemperatureLimit)
        {
            if (_activeTemperatureAlarms.ContainsKey(key))
                return;

            var alarm = new Alarm
            {
                EquipmentId = data.EquipmentId,
                AlarmCode = AlarmCode.TEMP_HIGH,
                AlarmMessage =
                    $"High Temperature Detected: {data.Temperature:F1} ℃",
                Severity = AlarmSeverity.Critical,
                OccurredAt = DateTime.Now,
                IsActive = true
            };

            alarm.Id =
                await _alarmRepository.InsertAsync(
                    alarm,
                    cancellationToken);

            _activeTemperatureAlarms[key] = alarm;

            await _hubContext.Clients.All.SendAsync(
                "AlarmRaised",
                alarm,
                cancellationToken);

            return;
        }

        if (_activeTemperatureAlarms.TryRemove(
                key,
                out var activeAlarm))
        {
            var clearedAt = DateTime.Now;

            await _alarmRepository.ClearAsync(
                activeAlarm.Id,
                clearedAt,
                cancellationToken);

            activeAlarm.IsActive = false;
            activeAlarm.ClearedAt = clearedAt;

            await _hubContext.Clients.All.SendAsync(
                "AlarmCleared",
                activeAlarm,
                cancellationToken);
        }
    }
}
