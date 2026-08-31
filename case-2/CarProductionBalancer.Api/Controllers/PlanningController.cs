using CarProductionBalancer.Api.DTOs;
using CarProductionBalancer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarProductionBalancer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlanningController : ControllerBase
{
    private readonly PlanningService _service;

    public PlanningController(PlanningService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePlanningRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed.",
                Errors = errors
            });
        }

        try
        {
            var (response, isNew) = await _service.CreateAsync(request);
            return isNew ? CreatedAtAction(nameof(GetById), new { id = response.PlanningId }, response) : Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed.",
                Errors = ex.Errors
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PlanningListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory()
    {
        var list = await _service.GetHistoryAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Planning with id '{id}' not found." });
        return Ok(result);
    }
}
