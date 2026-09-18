using EquipmentMonitoring.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace EquipmentMonitoring.Services;

public class AlarmApiService
{
    private readonly HttpClient _httpClient;

    public AlarmApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Alarm>> GetActiveAsync()
    {
        return await _httpClient
            .GetFromJsonAsync<List<Alarm>>(
                "api/alarm/active")
            ?? new List<Alarm>();
    }

    public async Task<List<Alarm>> GetHistoryAsync(
        string? equipmentId,
        DateTime from,
        DateTime to)
    {
        var url =
            $"api/alarm/history" +
            $"?from={Uri.EscapeDataString(from.ToString("O"))}" +
            $"&to={Uri.EscapeDataString(to.ToString("O"))}";

        if (!string.IsNullOrWhiteSpace(equipmentId))
        {
            url +=
                $"&equipmentId={Uri.EscapeDataString(equipmentId)}";
        }

        return await _httpClient
            .GetFromJsonAsync<List<Alarm>>(url)
            ?? new List<Alarm>();
    }
}
