using EquipmentMonitoring.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmController : ControllerBase
{
    private readonly IAlarmRepository _alarmRepository;

    public AlarmController(
        IAlarmRepository alarmRepository)
    {
        _alarmRepository = alarmRepository;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        var alarms =
            await _alarmRepository.GetActiveAsync(
                cancellationToken);

        return Ok(alarms);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        string? equipmentId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            return BadRequest(
                "from must be earlier than to.");
        }

        var alarms =
            await _alarmRepository.GetHistoryAsync(
                equipmentId,
                from.UtcDateTime,
                to.UtcDateTime,
                cancellationToken);

        return Ok(alarms);
    }
}
