using workstream.Data;
using workstream.Model;
using Microsoft.EntityFrameworkCore;


namespace workstream.Data
{
    public class InventoryRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<InventoryRepo> _logger;

        public InventoryRepo(WorkstreamDbContext context, ILogger<InventoryRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Inventory Item
        public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item, int tenantId)
        {
            if (item == null)
            {
                _logger.LogError("Inventory item data cannot be null.");
                throw new ArgumentNullException(nameof(item), "Inventory item data cannot be null.");
            }

            // Check if the tenant exists
            var tenantExists = await _context.Tenants.AnyAsync(t => t.TenantId == tenantId);
            if (!tenantExists)
            {
                _logger.LogError("Tenant with ID: {TenantId} does not exist.", tenantId);
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
            }

            item.TenantId = tenantId; // Ensure TenantId is set when creating the item
            item.IsDeleted = false;

            _logger.LogInformation("Creating inventory item with name: {Name} for tenant ID: {TenantId}", item.Name, tenantId);

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory item with ID: {ItemId} created successfully.", item.InventoryItemId);

            return item;
        }


        // Get a single Inventory Item by ID
        public async Task<InventoryItem> GetInventoryItemByIdAsync(int itemId, int tenantId)
        {
            if (itemId <= 0)
            {
                _logger.LogError("Invalid inventory item ID.");
                throw new ArgumentException("Invalid inventory item ID.", nameof(itemId));
            }

            _logger.LogInformation("Fetching inventory item with ID: {ItemId} for tenant ID: {TenantId}", itemId, tenantId);

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == itemId && i.TenantId == tenantId && !i.IsDeleted);

            if (item == null)
            {
                _logger.LogWarning("Inventory item with ID: {ItemId} not found for tenant ID: {TenantId}.", itemId, tenantId);
                throw new KeyNotFoundException($"Inventory item with ID {itemId} not found.");
            }

            return item;
        }

        // Get all Inventory Items for a Tenant
        public async Task<List<InventoryItem>> GetAllInventoryItemsAsync(int tenantId)
        {
            _logger.LogInformation("Fetching all inventory items for tenant ID: {TenantId}", tenantId);

            var items = await _context.InventoryItems
                .Where(i => i.TenantId == tenantId && !i.IsDeleted)
                .Include(i => i.Stocks) // Include the related Stocks navigation property
                .ToListAsync();

            _logger.LogInformation("Found {ItemCount} inventory items for tenant ID: {TenantId}", items.Count, tenantId);

            return items;
        }


        // Search Inventory Items by Name for a Tenant
        public async Task<List<InventoryItem>> SearchInventoryItemsAsync(string searchTerm, int tenantId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogInformation("No search term provided, fetching all inventory items for tenant ID: {TenantId}", tenantId);
                return await GetAllInventoryItemsAsync(tenantId);
            }

            _logger.LogInformation("Searching inventory items by name for tenant ID: {TenantId} with search term: {SearchTerm}", tenantId, searchTerm);

            var items = await _context.InventoryItems
                .Where(i => i.TenantId == tenantId && !i.IsDeleted && i.Name.Contains(searchTerm))
                .Include(i => i.Stocks) // Include the related Stocks navigation property
                .ToListAsync();

            _logger.LogInformation("Found {ItemCount} inventory items matching search term for tenant ID: {TenantId}", items.Count, tenantId);

            return items;
        }


        // Update an existing Inventory Item
        public async Task<InventoryItem> UpdateInventoryItemAsync(int itemId, InventoryItem updatedItem, int tenantId)
        {
            if (updatedItem == null)
            {
                _logger.LogError("Updated inventory item data cannot be null.");
                throw new ArgumentNullException(nameof(updatedItem), "Updated inventory item data cannot be null.");
            }

            _logger.LogInformation("Updating inventory item with ID: {ItemId} for tenant ID: {TenantId}", itemId, tenantId);

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == itemId && i.TenantId == tenantId && !i.IsDeleted);
            if (item == null)
            {
                _logger.LogWarning("Inventory item with ID: {ItemId} not found for tenant ID: {TenantId}.", itemId, tenantId);
                throw new KeyNotFoundException($"Inventory item with ID {itemId} not found.");
            }

            item.Name = updatedItem.Name ?? item.Name;
            item.Description = updatedItem.Description ?? item.Description;
            item.Price = updatedItem.Price != 0 ? updatedItem.Price : item.Price;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory item with ID: {ItemId} updated successfully for tenant ID: {TenantId}", itemId, tenantId);

            return item;
        }

        // Soft delete an Inventory Item
        public async Task<bool> DeleteInventoryItemAsync(int itemId, int tenantId)
        {
            _logger.LogInformation("Soft deleting inventory item with ID: {ItemId} for tenant ID: {TenantId}", itemId, tenantId);

            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == itemId && i.TenantId == tenantId && !i.IsDeleted);
            if (item == null)
            {
                _logger.LogWarning("Inventory item with ID: {ItemId} not found for tenant ID: {TenantId}.", itemId, tenantId);
                throw new KeyNotFoundException($"Inventory item with ID {itemId} not found.");
            }

            item.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory item with ID: {ItemId} soft deleted successfully for tenant ID: {TenantId}", itemId, tenantId);

            return true;
        }
    }
}
