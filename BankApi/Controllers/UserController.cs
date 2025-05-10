using BankApi.Models;
using BankApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository repository, ILogger<UserController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("cnp/{cnp}")]
        public async Task<ActionResult<User>> GetByCnp(string cnp)
        {
            var user = await _repository.GetByCnpAsync(cnp);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            var user = await _repository.GetByUsernameAsync(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            var createdUser = await _repository.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            var success = await _repository.UpdateAsync(user);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/punish")]
        public async Task<IActionResult> PunishUser(int id, [FromBody] PunishmentDetails details)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            // Update the user's properties for punishment
            user.GemBalance -= details.GemPenalty;
            user.NumberOfOffenses++;
            user.CreditScore -= 50; // Example punishment logic

            var success = await _repository.UpdateAsync(user);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

        [HttpPatch("{cnp}/creditScore")]
        public async Task<IActionResult> UpdateCreditScore(string cnp, [FromQuery] int newScore)
        {
            try
            {
                var user = await _repository.GetByCnpAsync(cnp);
                if (user == null)
                    return NotFound($"User with CNP {cnp} not found.");

                user.CreditScore = newScore;
                
                var success = await _repository.UpdateAsync(user);
                if (!success)
                    return NotFound();
                
                _logger.LogInformation($"Updated credit score for user with CNP {cnp} to {newScore}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating credit score for user with CNP {cnp}");
                return StatusCode(500, "An error occurred while updating the credit score.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }

    // Class to contain punishment details
    public class PunishmentDetails
    {
        public int GemPenalty { get; set; }
    }
}
