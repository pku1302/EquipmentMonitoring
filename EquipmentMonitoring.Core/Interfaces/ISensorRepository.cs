using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Core.Interfaces;

public interface ISensorRepository
{
    Task InsertAsync(
        SensorHistory history,
        CancellationToken cancellationToken = default);

    Task<List<SensorHistory>> GetHistoriesAsync(
        string equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
