// BankApi/Controllers/AlertsController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApi.Models;
using BankApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _repository;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(IAlertRepository repository, ILogger<AlertsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/Alerts
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAllAlerts()
        {
            try
            {
                var alerts = await _repository.GetAllAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all alerts");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data");
            }
        }

        // GET: api/Alerts/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Alert>> GetAlertById(int id)
        {
            try
            {
                var alert = await _repository.GetAlertByIdAsync(id);
                return Ok(alert);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Alert not found: {AlertId}", id);
                return NotFound($"Alert with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert by ID: {AlertId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data");
            }
        }

        // POST: api/Alerts
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Alert>> CreateAlert([FromBody] Alert alert)
        {
            try
            {
                if (alert == null)
                {
                    return BadRequest("Alert data is null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdAlert = await _repository.AddAlertAsync(alert);
                
                return CreatedAtAction(nameof(GetAlertById), 
                    new { id = createdAlert.AlertId }, 
                    createdAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert for stock: {StockName}", alert?.StockName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating alert");
            }
        }

        // PUT: api/Alerts/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAlert(int id, [FromBody] Alert alert)
        {
            try
            {
                if (alert == null)
                {
                    return BadRequest("Alert data is null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != alert.AlertId)
                {
                    return BadRequest("ID in URL does not match the alert ID in the body");
                }

                await _repository.UpdateAlertAsync(alert);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Alert not found during update: {AlertId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert: {AlertId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating alert");
            }
        }

        // DELETE: api/Alerts/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            try
            {
                var success = await _repository.DeleteAlertAsync(id);
                
                if (!success)
                {
                    return NotFound($"Alert with ID {id} not found");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert: {AlertId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting alert");
            }
        }

        // GET: api/Alerts/triggered
        [HttpGet("triggered")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TriggeredAlert>>> GetTriggeredAlerts()
        {
            try
            {
                var triggeredAlerts = await _repository.GetTriggeredAlertsAsync();
                return Ok(triggeredAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving triggered alerts");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data");
            }
        }

        // DELETE: api/Alerts/triggered
        [HttpDelete("triggered")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearTriggeredAlerts()
        {
            try
            {
                await _repository.ClearTriggeredAlertsAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing triggered alerts");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error clearing triggered alerts");
            }
        }

        // POST: api/Alerts/trigger
        [HttpPost("trigger")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TriggeredAlert>> TriggerAlert([FromBody] TriggerRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.StockName))
                {
                    return BadRequest("Stock name is required");
                }

                var triggeredAlert = await _repository.TriggerAlertAsync(request.StockName, request.CurrentPrice);
                
                if (triggeredAlert == null)
                {
                    return Ok(new { Message = "No alert was triggered" });
                }
                
                return Ok(triggeredAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering alert for stock: {StockName}", request.StockName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error triggering alert");
            }
        }
    }

    public class TriggerRequest
    {
        public string StockName { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
    }
}
