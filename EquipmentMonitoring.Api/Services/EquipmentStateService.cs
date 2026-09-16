using EquipmentMonitoring.Core.Models;
using System.Collections.Concurrent;

namespace EquipmentMonitoring.Api.Services;

public class EquipmentStateService
{
    private readonly ConcurrentDictionary<string, EquipmentData>
        _equipments = new();

    public void Update(EquipmentData data)
    {
        _equipments[data.EquipmentId] = data;
    }

    public EquipmentData? Get(string equipmentId)
    {
        _equipments.TryGetValue(
            equipmentId,
            out var equipment);

        return equipment;
    }

    public IReadOnlyCollection<EquipmentData> GetAll()
    {
        return _equipments.Values.ToList();
    }
}
