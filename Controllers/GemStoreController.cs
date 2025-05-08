using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Database;
using StockApp.Models;
using StockApp.Exceptions;

namespace StockApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GemStoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GemStoreController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GemStore>>> GetAllGemStores()
        {
            try
            {
                return await _context.GemStores.ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{cnp}")]
        public async Task<ActionResult<GemStore>> GetGemStoreByCnp(string cnp)
        {
            try
            {
                var gemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                if (gemStore == null)
                {
                    return NotFound($"Gem store for user with CNP {cnp} not found");
                }

                return gemStore;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<GemStore>> CreateGemStore(GemStore gemStore)
        {
            try
            {
                if (gemStore == null)
                {
                    return BadRequest("Gem store data is required");
                }

                gemStore.LastUpdated = DateTime.UtcNow;
                _context.GemStores.Add(gemStore);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetGemStoreByCnp), new { cnp = gemStore.UserCnp }, gemStore);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{cnp}")]
        public async Task<IActionResult> UpdateGemStore(string cnp, GemStore gemStore)
        {
            try
            {
                if (cnp != gemStore.UserCnp)
                {
                    return BadRequest("CNP mismatch");
                }

                var existingGemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                if (existingGemStore == null)
                {
                    return NotFound($"Gem store for user with CNP {cnp} not found");
                }

                existingGemStore.GemBalance = gemStore.GemBalance;
                existingGemStore.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{cnp}")]
        public async Task<IActionResult> DeleteGemStore(string cnp)
        {
            try
            {
                var gemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                if (gemStore == null)
                {
                    return NotFound($"Gem store for user with CNP {cnp} not found");
                }

                _context.GemStores.Remove(gemStore);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{cnp}/balance")]
        public async Task<ActionResult<int>> GetGemBalance(string cnp)
        {
            try
            {
                var gemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                if (gemStore == null)
                {
                    return NotFound($"Gem store for user with CNP {cnp} not found");
                }

                return gemStore.GemBalance;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 