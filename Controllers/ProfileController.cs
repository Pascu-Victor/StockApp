using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models;
using StockApp.Services;
using StockApp.Exceptions;

namespace StockApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        [HttpGet("{cnp}")]
        public async Task<ActionResult<Profile>> GetProfile(string cnp)
        {
            try
            {
                var profile = await _profileService.GetProfileByCnpAsync(cnp);
                return Ok(profile);
            }
            catch (ProfileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the profile.");
            }
        }

        [HttpGet("current")]
        public async Task<ActionResult<Profile>> GetCurrentProfile()
        {
            try
            {
                var profile = await _profileService.GetCurrentProfileAsync();
                return Ok(profile);
            }
            catch (ProfileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the current profile.");
            }
        }

        [HttpGet("generate-username")]
        public async Task<ActionResult<string>> GenerateUsername()
        {
            try
            {
                var username = await _profileService.GenerateUsernameAsync();
                return Ok(username);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while generating the username.");
            }
        }

        [HttpPut("{cnp}")]
        public async Task<IActionResult> UpdateProfile(string cnp, [FromBody] ProfileUpdateRequest request)
        {
            try
            {
                await _profileService.UpdateProfileAsync(cnp, request.Username, request.Image, request.Description, request.IsHidden);
                return NoContent();
            }
            catch (ProfileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the profile.");
            }
        }

        [HttpPut("{cnp}/admin")]
        public async Task<IActionResult> UpdateIsAdmin(string cnp, [FromBody] AdminUpdateRequest request)
        {
            try
            {
                await _profileService.UpdateIsAdminAsync(cnp, request.IsAdmin);
                return NoContent();
            }
            catch (ProfileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the admin status.");
            }
        }

        [HttpGet("{cnp}/stocks")]
        public async Task<ActionResult<List<Stock>>> GetUserStocks(string cnp)
        {
            try
            {
                var stocks = await _profileService.GetUserStocksAsync(cnp);
                return Ok(stocks);
            }
            catch (ProfileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving user stocks.");
            }
        }
    }

    public class ProfileUpdateRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
    }

    public class AdminUpdateRequest
    {
        public bool IsAdmin { get; set; }
    }
} 