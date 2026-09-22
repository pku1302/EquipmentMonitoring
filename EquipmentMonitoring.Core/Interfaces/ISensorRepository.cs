using EquipmentMonitoring.Core.Models;

namespace EquipmentMonitoring.Core.Interfaces;

public interface ISensorRepository
{
    Task InsertAsync(
        EquipmentData history,
        CancellationToken cancellationToken = default);

    Task<List<EquipmentData>> GetHistoriesAsync(
        string equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
