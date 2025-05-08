using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockApp.Exceptions;
using StockApp.Models;
using StockApp.Repositories;

namespace StockApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseStocksController : ControllerBase
    {
        private readonly IBaseStocksRepository _repository;

        public BaseStocksController(IBaseStocksRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // GET: api/BaseStocks
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BaseStock>>> GetAllStocks()
        {
            var stocks = await _repository.GetAllStocksAsync();
            return Ok(stocks);
        }

        // GET: api/BaseStocks/{name}
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BaseStock>> GetStock(string name)
        {
            try
            {
                var stock = await _repository.GetStockByNameAsync(name);
                return Ok(stock);
            }
            catch (StockNotFoundException)
            {
                return NotFound($"Stock with name '{name}' not found.");
            }
        }

        // POST: api/BaseStocks
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BaseStock>> CreateStock([FromBody] StockCreateDto stockDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                BaseStock stock = new()
                {
                    Name = stockDto.Name,
                    Symbol = stockDto.Symbol,
                    AuthorCNP = stockDto.AuthorCNP
                };
                var createdStock = await _repository.AddStockAsync(stock, stockDto.InitialPrice ?? 100);

                return CreatedAtAction(
                    nameof(GetStock),
                    new { name = createdStock.Name },
                    createdStock);
            }
            catch (DuplicateStockException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating stock: {ex.Message}");
            }
        }

        // PUT: api/BaseStocks/{name}
        [HttpPut("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStock(string name, [FromBody] StockUpdateDto stockDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (name != stockDto.Name)
            {
                return BadRequest("Stock name in URL must match stock name in request body.");
            }

            try
            {
                BaseStock stock = new()
                {
                    Name = stockDto.Name,
                    Symbol = stockDto.Symbol,
                    AuthorCNP = stockDto.AuthorCNP,
                };
                var updatedStock = await _repository.UpdateStockAsync(stock);
                return Ok(updatedStock);
            }
            catch (StockNotFoundException)
            {
                return NotFound($"Stock with name '{name}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating stock: {ex.Message}");
            }
        }

        // DELETE: api/BaseStocks/{name}
        [HttpDelete("{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStock(string name)
        {
            try
            {
                var result = await _repository.DeleteStockAsync(name);

                if (!result)
                {
                    return NotFound($"Stock with name '{name}' not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting stock: {ex.Message}");
            }
        }
    }

    public class StockCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 1)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public string AuthorCNP { get; set; } = string.Empty;

        public int? InitialPrice { get; set; }
    }

    public class StockUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 1)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public string AuthorCNP { get; set; } = string.Empty;
    }
}