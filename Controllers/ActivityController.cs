namespace StockApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using StockApp.Repositories;
    using Src.Model;

    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(IActivityRepository activityRepository, ILogger<ActivityController> logger)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetAllActivities()
        {
            try
            {
                var activities = await _activityRepository.GetAllActivitiesAsync();
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activities");
                return StatusCode(500, "An error occurred while retrieving activities");
            }
        }

        [HttpGet("user/{userCnp}")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetUserActivities(string userCnp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userCnp))
                {
                    return BadRequest("User CNP cannot be empty");
                }

                var activities = await _activityRepository.GetActivityForUserAsync(userCnp);
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

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityLog>> GetActivity(int id)
        {
            try
            {
                var activity = await _activityRepository.GetActivityByIdAsync(id);
                if (activity == null)
                {
                    return NotFound();
                }
                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the activity");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ActivityLog>> CreateActivity([FromBody] ActivityLog activity)
        {
            try
            {
                await _activityRepository.AddActivityAsync(
                    activity.UserCnp,
                    activity.ActivityName,
                    activity.LastModifiedAmount,
                    activity.ActivityDetails);

                return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return StatusCode(500, "An error occurred while creating the activity");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                await _activityRepository.DeleteActivityAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity {Id}", id);
                return StatusCode(500, "An error occurred while deleting the activity");
            }
        }
    }
} 