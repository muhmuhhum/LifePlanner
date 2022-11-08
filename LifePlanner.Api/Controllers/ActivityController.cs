using LifePlanner.Api.Domain;
using LifePlanner.Api.Store;
using Microsoft.AspNetCore.Mvc;

namespace LifePlanner.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityController : ControllerBase
{
    private readonly ILogger<ActivityController> _logger;
    private readonly IActivityStore _manager;


    public ActivityController(ILogger<ActivityController> logger, IActivityStore manager)
    {
        _logger = logger;
        _manager = manager;
    }

    [HttpPost]
    public IActionResult Create(Activity activity)
    {
        var result = _manager.CreateAsync(activity);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _manager.GetAll();
            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogControllerError(exception);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _manager.GetById(id);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogControllerError(exception);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(Activity activity)
    {
        try
        {
            var result = await _manager.Update(activity);
            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogControllerError(exception);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _manager.Delete(id);
            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogControllerError(exception);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}