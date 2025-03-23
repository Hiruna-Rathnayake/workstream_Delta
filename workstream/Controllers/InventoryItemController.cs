using Microsoft.AspNetCore.Mvc;
using workstream.Model;
using workstream.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using workstream.DTO;
using workstream.Services;

namespace workstream.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryRepo _inventoryRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryController> _logger;
        private readonly JwtService _jwtService;

        public InventoryController(InventoryRepo inventoryRepo, IMapper mapper, ILogger<InventoryController> logger, JwtService jwtService)
        {
            _inventoryRepo = inventoryRepo;
            _mapper = mapper;
            _logger = logger;
            _jwtService = jwtService;
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<IActionResult> CreateInventoryItem([FromBody] InventoryItemWriteDTO inventoryItemDto)
        {
            if (inventoryItemDto == null)
            {
                _logger.LogError("\x1b[33mInventory item data is null.\x1b[0m");
                return BadRequest("Inventory item data cannot be null.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var hasPermission = await _jwtService.UserHasPermissionAsync(token, "InventoryManagement");

            if (!hasPermission)
            {
                _logger.LogWarning("\x1b[33mUser does not have 'InventoryManagement' permission.\x1b[0m");
                return Forbid("You do not have permission to manage inventory.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token);
            _logger.LogInformation("\x1b[33mCreating inventory item for tenant {TenantId}.\x1b[0m", tenantId);

            var inventoryItem = _mapper.Map<InventoryItem>(inventoryItemDto);
            var createdItem = await _inventoryRepo.CreateInventoryItemAsync(inventoryItem, tenantId);
            var resultDto = _mapper.Map<InventoryItemReadDTO>(createdItem);

            _logger.LogInformation("\x1b[33mInventory item {ItemId} created for tenant {TenantId}.\x1b[0m", resultDto.InventoryItemId, tenantId);

            return CreatedAtAction(nameof(GetInventoryItem), new { itemId = resultDto.InventoryItemId }, resultDto);
        }

        // GET: api/inventory/{id}
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetInventoryItem(int itemId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var hasPermission = await _jwtService.UserHasPermissionAsync(token, "InventoryManagement");

            if (!hasPermission)
            {
                _logger.LogWarning("\x1b[33mUser does not have 'InventoryManagement' permission.\x1b[0m");
                return Forbid("You do not have permission to view inventory.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token);
            _logger.LogInformation("\x1b[33mFetching inventory item {ItemId} for tenant {TenantId}.\x1b[0m", itemId, tenantId);

            try
            {
                var item = await _inventoryRepo.GetInventoryItemByIdAsync(itemId, tenantId);
                var itemDto = _mapper.Map<InventoryItemReadDTO>(item);

                _logger.LogInformation("\x1b[33mInventory item {ItemId} retrieved for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return Ok(itemDto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("\x1b[33mInventory item {ItemId} not found for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return NotFound($"Inventory item with ID {itemId} not found.");
            }
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var hasPermission = await _jwtService.UserHasPermissionAsync(token, "InventoryManagement");

            if (!hasPermission)
            {
                _logger.LogWarning("\x1b[33mUser does not have 'InventoryManagement' permission.\x1b[0m");
                return Forbid("You do not have permission to view inventory.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token);
            _logger.LogInformation("\x1b[33mFetching all inventory items for tenant {TenantId}.\x1b[0m", tenantId);

            var items = await _inventoryRepo.GetAllInventoryItemsAsync(tenantId);
            var itemsDto = _mapper.Map<List<InventoryItemReadDTO>>(items);

            _logger.LogInformation("\x1b[33mFound {ItemCount} inventory items for tenant {TenantId}.\x1b[0m", itemsDto.Count, tenantId);
            return Ok(itemsDto);
        }

        // PUT: api/inventory/{id}
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateInventoryItem(int itemId, [FromBody] InventoryItemWriteDTO inventoryItemDto)
        {
            if (inventoryItemDto == null)
            {
                _logger.LogError("\x1b[33mUpdated inventory item data is null.\x1b[0m");
                return BadRequest("Updated inventory item data cannot be null.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var hasPermission = await _jwtService.UserHasPermissionAsync(token, "InventoryManagement");

            if (!hasPermission)
            {
                _logger.LogWarning("\x1b[33mUser does not have 'InventoryManagement' permission.\x1b[0m");
                return Forbid("You do not have permission to update inventory.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token);
            _logger.LogInformation("\x1b[33mUpdating inventory item {ItemId} for tenant {TenantId}.\x1b[0m", itemId, tenantId);

            var inventoryItem = _mapper.Map<InventoryItem>(inventoryItemDto);

            try
            {
                var updatedItem = await _inventoryRepo.UpdateInventoryItemAsync(itemId, inventoryItem, tenantId);
                var resultDto = _mapper.Map<InventoryItemReadDTO>(updatedItem);

                _logger.LogInformation("\x1b[33mInventory item {ItemId} updated for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return Ok(resultDto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("\x1b[33mInventory item {ItemId} not found for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return NotFound($"Inventory item with ID {itemId} not found.");
            }
        }

        // DELETE: api/inventory/{id}
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteInventoryItem(int itemId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var hasPermission = await _jwtService.UserHasPermissionAsync(token, "InventoryManagement");

            if (!hasPermission)
            {
                _logger.LogWarning("\x1b[33mUser does not have 'InventoryManagement' permission.\x1b[0m");
                return Forbid("You do not have permission to delete inventory.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token);
            _logger.LogInformation("\x1b[33mDeleting inventory item {ItemId} for tenant {TenantId}.\x1b[0m", itemId, tenantId);

            try
            {
                var isDeleted = await _inventoryRepo.DeleteInventoryItemAsync(itemId, tenantId);

                if (isDeleted)
                {
                    _logger.LogInformation("\x1b[33mInventory item {ItemId} deleted for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                    return Ok(new { message = "Item deleted successfully" });
                }

                _logger.LogWarning("\x1b[33mInventory item {ItemId} not found for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return NotFound($"Inventory item with ID {itemId} not found.");
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("\x1b[33mInventory item {ItemId} not found for tenant {TenantId}.\x1b[0m", itemId, tenantId);
                return NotFound($"Inventory item with ID {itemId} not found.");
            }
        }
    }
}
