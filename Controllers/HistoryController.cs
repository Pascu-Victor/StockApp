using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models;
using StockApp.Repositories;

namespace StockApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryController(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        }

        [HttpGet("user/{userCnp}")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryForUser(string userCnp)
        {
            try
            {
                var history = await _historyRepository.GetHistoryForUserAsync(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the credit score history.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreditScoreHistory>> AddHistoryEntry(CreditScoreHistory history)
        {
            try
            {
                var result = await _historyRepository.AddHistoryEntryAsync(history);
                return CreatedAtAction(nameof(GetHistoryForUser), new { userCnp = result.UserCnp }, result);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("History entry cannot be null.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the credit score history entry.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistoryEntry(int id)
        {
            try
            {
                var result = await _historyRepository.DeleteHistoryEntryAsync(id);
                if (!result)
                {
                    return NotFound($"History entry with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the credit score history entry.");
            }
        }
    }
} 