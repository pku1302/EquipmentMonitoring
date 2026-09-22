using EquipmentMonitoring.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EquipmentMonitoring.Services;

public class SensorApiService
{
    private readonly HttpClient _httpClient;
    public SensorApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<EquipmentData>> GetHistoryAsync(
        string equipmentId,
        DateTime from,
        DateTime to)
    {
        var url =
            $"api/sensor/history" +
            $"?equipmentId={Uri.EscapeDataString(equipmentId)}" +
            $"&from={Uri.EscapeDataString(from.ToString("O"))}" +
            $"&to={Uri.EscapeDataString(to.ToString("O"))}";

        return await _httpClient
            .GetFromJsonAsync<List<EquipmentData>>(url) 
            ?? [];
    }
}
