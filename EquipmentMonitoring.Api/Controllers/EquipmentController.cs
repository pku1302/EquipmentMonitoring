using EquipmentMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentStateService _stateService;

    public EquipmentController(
        EquipmentStateService stateService)
    {
        _stateService = stateService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var equipments = _stateService.GetAll();

        return Ok(equipments);
    }

    [HttpGet("{equipmentId}")]
    public IActionResult Get(
        string equipmentId)
    {
        var equipment =
            _stateService.Get(equipmentId);

        if (equipment == null)
        {
            return NotFound();
        }

        return Ok(equipment);
    }
}
