using EquipmentMonitoring.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly SensorRepository _sensorRepository;

    public SensorController(
        SensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        string equipmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            return BadRequest(
                "from must be earlier than to.");
        }

        var history =
            await _sensorRepository.GetHistoriesAsync(
                equipmentId,
                from,
                to,
                cancellationToken);

        return Ok(history);
    }
}
