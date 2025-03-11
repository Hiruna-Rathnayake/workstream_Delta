using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using workstream.Data;
using workstream.Model;
using workstream.DTO;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly StockRepo _stockRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<StockController> _logger;

        public StockController(StockRepo stockRepo, IMapper mapper, ILogger<StockController> logger)
        {
            _stockRepo = stockRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // Create Stock
        [HttpPost]
        public async Task<IActionResult> CreateStockAsync([FromBody] StockWriteDTO stockWriteDto)
        {
            if (stockWriteDto == null)
            {
                _logger.LogWarning("CreateStockAsync called with null data.");
                return BadRequest("Stock data is required.");
            }

            try
            {
                _logger.LogInformation("Creating stock for InventoryItem ID: {InventoryItemId}", stockWriteDto.InventoryItemId);
                var stock = _mapper.Map<Stock>(stockWriteDto);
                var createdStock = await _stockRepo.CreateStockAsync(stock);
                var stockReadDto = _mapper.Map<StockReadDTO>(createdStock);

                _logger.LogInformation("Stock created successfully with ID: {StockId}", stockReadDto.StockId);
                return CreatedAtAction(nameof(GetStockById), new { id = stockReadDto.StockId }, stockReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock.");
                return StatusCode(500, "An error occurred while creating the stock.");
            }
        }

        // Get Stock by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetStockById called with invalid ID: {StockId}", id);
                return BadRequest("Invalid stock ID.");
            }

            try
            {
                _logger.LogInformation("Fetching stock with ID: {StockId}", id);
                var stock = await _stockRepo.GetStockByIdAsync(id);
                var stockReadDto = _mapper.Map<StockReadDTO>(stock);
                return Ok(stockReadDto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Stock with ID {StockId} not found.", id);
                return NotFound($"Stock with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock by ID: {StockId}", id);
                return StatusCode(500, "An error occurred while fetching the stock.");
            }
        }

        // Get all Stocks for a Tenant
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetAllStocksByTenantId(int tenantId)
        {
            if (tenantId <= 0)
            {
                _logger.LogWarning("GetAllStocksByTenantId called with invalid tenant ID: {TenantId}", tenantId);
                return BadRequest("Invalid tenant ID.");
            }

            try
            {
                _logger.LogInformation("Fetching all stocks for tenant ID: {TenantId}", tenantId);
                var stocks = await _stockRepo.GetAllStocksByTenantIdAsync(tenantId);
                var stockReadDtos = _mapper.Map<List<StockReadDTO>>(stocks);
                return Ok(stockReadDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocks for tenant ID: {TenantId}", tenantId);
                return StatusCode(500, "An error occurred while fetching stocks for the tenant.");
            }
        }

        // Update Stock
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStockAsync(int id, [FromBody] StockWriteDTO stockWriteDto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("UpdateStockAsync called with invalid stock ID: {StockId}", id);
                return BadRequest("Invalid stock ID.");
            }

            if (stockWriteDto == null)
            {
                _logger.LogWarning("UpdateStockAsync called with null data.");
                return BadRequest("Stock data is required.");
            }

            try
            {
                _logger.LogInformation("Updating stock with ID: {StockId}", id);
                var stock = _mapper.Map<Stock>(stockWriteDto);
                stock.StockId = id;
                var updatedStock = await _stockRepo.UpdateStockAsync(id, stock);
                var stockReadDto = _mapper.Map<StockReadDTO>(updatedStock);

                _logger.LogInformation("Stock with ID: {StockId} updated successfully.", id);
                return Ok(stockReadDto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Stock with ID {StockId} not found.", id);
                return NotFound($"Stock with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock with ID: {StockId}", id);
                return StatusCode(500, "An error occurred while updating the stock.");
            }
        }

        // Delete Stock
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("DeleteStockAsync called with invalid stock ID: {StockId}", id);
                return BadRequest("Invalid stock ID.");
            }

            try
            {
                _logger.LogInformation("Deleting stock with ID: {StockId}", id);
                var deleted = await _stockRepo.DeleteStockAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Stock with ID {StockId} not found.", id);
                    return NotFound($"Stock with ID {id} not found.");
                }

                _logger.LogInformation("Stock with ID: {StockId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock with ID: {StockId}", id);
                return StatusCode(500, "An error occurred while deleting the stock.");
            }
        }
    }
}
