using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using workstream.Model;

namespace workstream.Data
{
    public class StockRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<StockRepo> _logger;

        public StockRepo(WorkstreamDbContext context, ILogger<StockRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Stock entry
        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            if (stock == null)
            {
                _logger.LogError("Stock data cannot be null.");
                throw new ArgumentNullException(nameof(stock), "Stock data cannot be null.");
            }

            // Ensure that the associated InventoryItem exists
            var inventoryItemExists = await _context.InventoryItems
                .AnyAsync(i => i.InventoryItemId == stock.InventoryItemId && !i.IsDeleted);

            if (!inventoryItemExists)
            {
                _logger.LogError("Inventory item with ID: {InventoryItemId} not found or deleted.", stock.InventoryItemId);
                throw new KeyNotFoundException($"Inventory item with ID {stock.InventoryItemId} not found or deleted.");
            }

            _logger.LogInformation("Creating stock for inventory item with ID: {InventoryItemId}.", stock.InventoryItemId);

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock with ID: {StockId} created successfully for inventory item ID: {InventoryItemId}.", stock.StockId, stock.InventoryItemId);
            return stock;
        }

        // Get Stock by ID
        public async Task<Stock> GetStockByIdAsync(int stockId)
        {
            if (stockId <= 0)
            {
                _logger.LogError("Invalid stock ID: {StockId}.", stockId);
                throw new ArgumentException("Invalid stock ID.", nameof(stockId));
            }

            _logger.LogInformation("Fetching stock with ID: {StockId}.", stockId);

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.StockId == stockId);

            if (stock == null)
            {
                _logger.LogWarning("Stock with ID: {StockId} not found.", stockId);
                throw new KeyNotFoundException($"Stock with ID {stockId} not found.");
            }

            return stock;
        }


        // Get all Stocks for a given InventoryItem ID
        public async Task<List<Stock>> GetAllStocksByInventoryItemIdAsync(int inventoryItemId)
        {
            if (inventoryItemId <= 0)
            {
                _logger.LogError("Invalid inventory item ID: {InventoryItemId}.", inventoryItemId);
                throw new ArgumentException("Invalid inventory item ID.", nameof(inventoryItemId));
            }

            _logger.LogInformation("Fetching all stocks for inventory item with ID: {InventoryItemId}.", inventoryItemId);

            var stocks = await _context.Stocks
                .Where(s => s.InventoryItemId == inventoryItemId)
                .ToListAsync();

            _logger.LogInformation("Found {StockCount} stocks for inventory item ID: {InventoryItemId}.", stocks.Count, inventoryItemId);

            return stocks;
        }

        // Get all Stocks for a given Tenant
        public async Task<List<Stock>> GetAllStocksByTenantIdAsync(int tenantId)
        {
            if (tenantId <= 0)
            {
                _logger.LogError("Invalid tenant ID: {TenantId}.", tenantId);
                throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));
            }

            _logger.LogInformation("Fetching all stocks for tenant ID: {TenantId}.", tenantId);

            var stocks = await _context.Stocks
                .Where(s => s.InventoryItemId != null) // Ensure Stock has a valid InventoryItemId
                .Join(_context.InventoryItems,
                      stock => stock.InventoryItemId,
                      inventoryItem => inventoryItem.InventoryItemId,
                      (stock, inventoryItem) => new { Stock = stock, InventoryItem = inventoryItem })
                .Where(x => x.InventoryItem.TenantId == tenantId)
                .Select(x => x.Stock)  // Select only the Stock object
                .ToListAsync();

            _logger.LogInformation("Found {StockCount} stocks for tenant ID: {TenantId}.", stocks.Count, tenantId);

            return stocks;
        }


        // Update Stock details
        public async Task<Stock> UpdateStockAsync(int stockId, Stock updatedStock)
        {
            if (updatedStock == null)
            {
                _logger.LogError("Updated stock data cannot be null.");
                throw new ArgumentNullException(nameof(updatedStock), "Updated stock data cannot be null.");
            }

            if (stockId != updatedStock.StockId)
            {
                _logger.LogError("Stock ID mismatch: expected {StockId}, received {UpdatedStockId}.", stockId, updatedStock.StockId);
                throw new ArgumentException("Stock ID mismatch.", nameof(stockId));
            }

            _logger.LogInformation("Updating stock with ID: {StockId}.", stockId);

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.StockId == stockId);

            if (stock == null)
            {
                _logger.LogWarning("Stock with ID: {StockId} not found.", stockId);
                throw new KeyNotFoundException($"Stock with ID {stockId} not found.");
            }

            // Update fields
            stock.Quantity = updatedStock.Quantity != 0 ? updatedStock.Quantity : stock.Quantity;
            stock.ManufacturingDate = updatedStock.ManufacturingDate != default ? updatedStock.ManufacturingDate : stock.ManufacturingDate;
            stock.ExpirationDate = updatedStock.ExpirationDate != default ? updatedStock.ExpirationDate : stock.ExpirationDate;
            stock.BatchNumber = updatedStock.BatchNumber ?? stock.BatchNumber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock with ID: {StockId} updated successfully.", stockId);
            return stock;
        }

        // Hard delete Stock
        public async Task<bool> DeleteStockAsync(int stockId)
        {
            if (stockId <= 0)
            {
                _logger.LogError("Invalid stock ID: {StockId}.", stockId);
                throw new ArgumentException("Invalid stock ID.", nameof(stockId));
            }

            _logger.LogInformation("Deleting stock with ID: {StockId}.", stockId);

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.StockId == stockId);

            if (stock == null)
            {
                _logger.LogWarning("Stock with ID: {StockId} not found.", stockId);
                throw new KeyNotFoundException($"Stock with ID {stockId} not found.");
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock with ID: {StockId} deleted successfully.", stockId);
            return true;
        }
    }
}
