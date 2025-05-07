namespace StockApp.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Database;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpGet("{cnp}")]
        public async Task<ActionResult<User>> GetUserByCnp(string cnp)
        {
            try
            {
                var user = await userRepository.GetUserByCnpAsync(cnp);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No user found with CNP: {cnp}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            try
            {
                var user = await userRepository.GetUserByUsernameAsync(username);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No user found with username: {username}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await userRepository.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }

            try
            {
                await userRepository.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserByCnp), new { cnp = user.CNP }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{cnp}")]
        public async Task<ActionResult> UpdateUser(string cnp, [FromBody] User user)
        {
            if (user == null || cnp != user.CNP)
            {
                return BadRequest("Invalid user or CNP mismatch.");
            }

            try
            {
                await userRepository.UpdateUserAsync(user);
                return NoContent(); // 204 - Success but no response body
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{cnp}")]
        public async Task<ActionResult> DeleteUser(string cnp)
        {
            try
            {
                await userRepository.DeleteUserAsync(cnp);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No user found with CNP: {cnp}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{cnp}/penalize/{amount}")]
        public async Task<ActionResult> PenalizeUser(string cnp, int amount)
        {
            try
            {
                await userRepository.PenalizeUserAsync(cnp, amount);
                return Ok($"User {cnp} penalized by {amount} points.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{cnp}/increment-offenses")]
        public async Task<ActionResult> IncrementOffenses(string cnp)
        {
            try
            {
                await userRepository.IncrementOffensesCountAsync(cnp);
                return Ok($"Offenses incremented for user {cnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{cnp}/update-creditscore/{creditScore}")]
        public async Task<ActionResult> UpdateCreditScore(string cnp, int creditScore)
        {
            try
            {
                await userRepository.UpdateUserCreditScoreAsync(cnp, creditScore);
                return Ok($"Credit score updated for {cnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{cnp}/update-roi/{roi}")]
        public async Task<ActionResult> UpdateROI(string cnp, decimal roi)
        {
            try
            {
                await userRepository.UpdateUserROIAsync(cnp, roi);
                return Ok($"ROI updated for {cnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{cnp}/update-riskscore/{riskScore}")]
        public async Task<ActionResult> UpdateRiskScore(string cnp, int riskScore)
        {
            try
            {
                await userRepository.UpdateUserRiskScoreAsync(cnp, riskScore);
                return Ok($"Risk score updated for {cnp}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
