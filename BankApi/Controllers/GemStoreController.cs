using BankApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GemStoreController : ControllerBase
    {
        private readonly IGemStoreRepository _gemStoreRepository;

        public GemStoreController(IGemStoreRepository gemStoreRepository)
        {
            _gemStoreRepository = gemStoreRepository ?? throw new ArgumentNullException(nameof(gemStoreRepository));
        }

        [HttpGet("balance/{cnp}")]
        public async Task<ActionResult<int>> GetUserGemBalance(string cnp)
        {
            try
            {
                var balance = await _gemStoreRepository.GetUserGemBalanceAsync(cnp);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("balance/{cnp}")]
        public async Task<ActionResult> UpdateUserGemBalance(string cnp, [FromBody] int newBalance)
        {
            try
            {
                await _gemStoreRepository.UpdateUserGemBalanceAsync(cnp, newBalance);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}