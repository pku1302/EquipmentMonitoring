using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Core.Interfaces;

public interface IAlarmRepository
{
    Task<long> InsertAsync(
        Alarm alarm,
        CancellationToken cancellationToken = default);

    Task ClearAsync(
        long alarmId,
        DateTime clearedAt,
        CancellationToken cancellationToken = default);

    Task<List<Alarm>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<List<Alarm>> GetHistoryAsync(
        string? equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
