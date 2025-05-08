namespace StockApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using StockApp.Exceptions;
    using StockApp.Models;
    using StockApp.Repositories;

    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryController(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        }

        [HttpGet]
        public ActionResult<List<CreditScoreHistory>> GetAllHistory()
        {
            try
            {
                var history = _historyRepository.GetAllHistory();
                return Ok(history);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the credit score history.");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<CreditScoreHistory> GetHistoryById(int id)
        {
            try
            {
                var history = _historyRepository.GetHistoryById(id);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the credit score history entry.");
            }
        }

        [HttpGet("user/{userCnp}")]
        public ActionResult<List<CreditScoreHistory>> GetHistoryForUser(string userCnp)
        {
            try
            {
                var history = _historyRepository.GetHistoryForUser(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the credit score history.");
            }
        }

        [HttpGet("user/{userCnp}/weekly")]
        public ActionResult<List<CreditScoreHistory>> GetHistoryWeekly(string userCnp)
        {
            try
            {
                var history = _historyRepository.GetHistoryWeekly(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the weekly credit score history.");
            }
        }

        [HttpGet("user/{userCnp}/monthly")]
        public ActionResult<List<CreditScoreHistory>> GetHistoryMonthly(string userCnp)
        {
            try
            {
                var history = _historyRepository.GetHistoryMonthly(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the monthly credit score history.");
            }
        }

        [HttpGet("user/{userCnp}/yearly")]
        public ActionResult<List<CreditScoreHistory>> GetHistoryYearly(string userCnp)
        {
            try
            {
                var history = _historyRepository.GetHistoryYearly(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the yearly credit score history.");
            }
        }

        [HttpPost]
        public ActionResult<CreditScoreHistory> AddHistory(CreditScoreHistory history)
        {
            try
            {
                _historyRepository.AddHistory(history);
                return CreatedAtAction(nameof(GetHistoryById), new { id = history.Id }, history);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("History entry cannot be null.");
            }
            catch (HistoryServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the credit score history entry.");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateHistory(int id, CreditScoreHistory history)
        {
            if (id != history.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _historyRepository.UpdateHistory(history);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("History entry cannot be null.");
            }
            catch (HistoryServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the credit score history entry.");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteHistory(int id)
        {
            try
            {
                _historyRepository.DeleteHistory(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the credit score history entry.");
            }
        }
    }
} 