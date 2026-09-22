using EquipmentMonitoring.Core.Interfaces;
using EquipmentMonitoring.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ISensorRepository _sensorRepository;

    public SensorController(
        ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        string equipmentId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            if (from > to)
            {
                return BadRequest(
                    "from must be earlier than to.");
            }

            var history =
                await _sensorRepository.GetHistoriesAsync(
                    equipmentId,
                    from.UtcDateTime,
                    to.UtcDateTime,
                    cancellationToken);

            return Ok(history);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return StatusCode(
                500,
                ex.Message);
        }
    }
}
