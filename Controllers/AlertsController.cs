using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Exceptions;

namespace StockApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;

        public AlertsController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        }

        /// <summary>
        /// Gets all alerts.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<Alert>> GetAll()
        {
            return Ok(_alertRepository.GetAllAlerts());
        }

        /// <summary>
        /// Gets an alert by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<Alert> GetById(int id)
        {
            try
            {
                var alert = _alertRepository.GetAlertById(id);
                return Ok(alert);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Creates a new alert.
        /// </summary>
        [HttpPost]
        public ActionResult<Alert> Create(Alert alert)
        {
            if (alert == null)
                return BadRequest("Alert cannot be null.");

            try
            {
                _alertRepository.AddAlert(alert);
                return CreatedAtAction(nameof(GetById), new { id = alert.AlertId }, alert);
            }
            catch (Exception ex)
            {
                return BadRequest(new AlertRepositoryException("Failed to create alert", ex).Message);
            }
        }

        /// <summary>
        /// Updates an existing alert.
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(int id, Alert alert)
        {
            if (alert == null || id != alert.AlertId)
                return BadRequest("Invalid alert data.");

            try
            {
                _alertRepository.UpdateAlert(alert);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new AlertRepositoryException("Failed to update alert", ex).Message);
            }
        }

        /// <summary>
        /// Deletes an alert.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _alertRepository.DeleteAlert(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Triggers an alert for a stock at a given price.
        /// </summary>
        [HttpPost("{stockName}/trigger")]
        public IActionResult TriggerAlert(string stockName, [FromBody] decimal currentPrice)
        {
            try
            {
                _alertRepository.TriggerAlert(stockName, currentPrice);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new AlertRepositoryException("Failed to trigger alert", ex).Message);
            }
        }

        /// <summary>
        /// Gets all triggered alerts.
        /// </summary>
        [HttpGet("triggered")]
        public ActionResult<IEnumerable<TriggeredAlert>> GetTriggeredAlerts()
        {
            return Ok(_alertRepository.GetTriggeredAlerts());
        }

        /// <summary>
        /// Clears all triggered alerts.
        /// </summary>
        [HttpDelete("triggered")]
        public IActionResult ClearTriggeredAlerts()
        {
            _alertRepository.ClearTriggeredAlerts();
            return NoContent();
        }
    }
} 