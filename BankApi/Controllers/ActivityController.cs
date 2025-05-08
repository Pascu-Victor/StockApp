using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApi.Models;
using BankApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _repository;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(IActivityRepository repository, ILogger<ActivityController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityLog>>> GetAllActivities()
        {
            try
            {
                var activities = await _repository.GetAllActivitiesAsync();
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activities");
                return StatusCode(500, "An error occurred while retrieving activities");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityLog>> GetActivityById(int id)
        {
            try
            {
                var activity = await _repository.GetActivityByIdAsync(id);
                return Ok(activity);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Activity with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity with ID {ActivityId}", id);
                return StatusCode(500, "An error occurred while retrieving the activity");
            }
        }

        [HttpGet("user/{userCnp}")]
        public async Task<ActionResult<List<ActivityLog>>> GetActivitiesForUser(string userCnp)
        {
            try
            {
                var activities = await _repository.GetActivityForUserAsync(userCnp);
                return Ok(activities);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user {UserCnp}", userCnp);
                return StatusCode(500, "An error occurred while retrieving user activities");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ActivityLog>> AddActivity([FromBody] ActivityLog activity)
        {
            try
            {
                var newActivity = await _repository.AddActivityAsync(
                    activity.UserCnp,
                    activity.ActivityName,
                    activity.LastModifiedAmount,
                    activity.ActivityDetails);

                return CreatedAtAction(nameof(GetActivityById), new { id = newActivity.Id }, newActivity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new activity");
                return StatusCode(500, "An error occurred while adding the activity");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                var result = await _repository.DeleteActivityAsync(id);
                if (!result)
                {
                    return NotFound($"Activity with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ActivityId}", id);
                return StatusCode(500, "An error occurred while deleting the activity");
            }
        }
    }
} 