using Microsoft.EntityFrameworkCore;
using workstream.Model;
using Microsoft.Extensions.Logging;
using workstream.DTO;

namespace workstream.Data
{
    public class OrderRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<OrderRepo> _logger;

        public OrderRepo(WorkstreamDbContext context, ILogger<OrderRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Order with at least one OrderItem
        public async Task<Order> CreateOrderAsync(Order order, int tenantId)
        {
            if (order == null)
            {
                _logger.LogError("Order data cannot be null.");
                throw new ArgumentNullException(nameof(order), "Order data cannot be null.");
            }

            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                _logger.LogError("An order must have at least one OrderItem.");
                throw new ArgumentException("An order must have at least one OrderItem.");
            }

            var tenantExists = await _context.Tenants.AnyAsync(t => t.TenantId == tenantId);
            if (!tenantExists)
            {
                _logger.LogError("Tenant with ID: {TenantId} does not exist.", tenantId);
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
            }

            order.TenantId = tenantId;

            foreach (var orderItem in order.OrderItems)
            {
                orderItem.OrderId = order.OrderId;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order with ID: {OrderId} created successfully with {OrderItemCount} items.", order.OrderId, order.OrderItems.Count);

            return order;
        }

        // Get Order by ID and include all related OrderItems
        public async Task<Order> GetOrderByIdAsync(int orderId, int tenantId)
        {
            if (orderId <= 0)
            {
                _logger.LogError("Invalid order ID.");
                throw new ArgumentException("Invalid order ID.", nameof(orderId));
            }

            _logger.LogInformation("Fetching order with ID: {OrderId} for tenant ID: {TenantId}", orderId, tenantId);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == tenantId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID: {OrderId} not found for tenant ID: {TenantId}.", orderId, tenantId);
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            return order;
        }

        // Update an Order (add/remove items, update status)
        public async Task<Order> UpdateOrderAsync(int orderId, int tenantId, OrderWriteDTO orderDto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == tenantId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found for tenant {tenantId}.");
            }

            // Update Order Status
            if (!string.IsNullOrEmpty(orderDto.Status))
            {
                order.Status = orderDto.Status;
            }

            // Add new OrderItems
            foreach (var item in orderDto.ItemsToAdd)
            {
                var newOrderItem = new OrderItem
                {
                    OrderId = orderId,
                    InventoryItemId = item.InventoryItemId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderItems.Add(newOrderItem);
            }

            // Remove specified OrderItems
            foreach (var itemId in orderDto.ItemsToRemove)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.OrderItemId == itemId);
                if (orderItem != null)
                {
                    _context.OrderItems.Remove(orderItem);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order with ID: {OrderId} updated successfully for tenant ID: {TenantId}.", orderId, tenantId);

            return order;
        }

        // Add an InventoryItem to an existing Order (single item)
        public async Task<OrderItem> AddInventoryItemToOrderAsync(int orderId, int inventoryItemId, int quantity, int tenantId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TenantId == tenantId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == inventoryItemId && i.TenantId == tenantId);

            if (inventoryItem == null)
            {
                throw new KeyNotFoundException($"Inventory item with ID {inventoryItemId} not found.");
            }

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                InventoryItemId = inventoryItem.InventoryItemId,
                Quantity = quantity,
                Price = inventoryItem.Price
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            return orderItem;
        }

        // Fetch all OrderItems related to a specific Order
        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId, int tenantId)
        {
            _logger.LogInformation("Fetching order items for order ID: {OrderId} for tenant ID: {TenantId}", orderId, tenantId);

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId && oi.Order.TenantId == tenantId)
                .ToListAsync();

            return orderItems;
        }

        // Fetch all Orders for a specific Tenant
        public async Task<List<Order>> GetAllOrdersByTenantIdAsync(int tenantId)
        {
            if (tenantId <= 0)
            {
                _logger.LogError("Invalid tenant ID.");
                throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));
            }

            _logger.LogInformation("Fetching all orders for tenant ID: {TenantId}", tenantId);

            var orders = await _context.Orders
                .Where(o => o.TenantId == tenantId)
                .Include(o => o.OrderItems) // Eagerly load OrderItems
                    .ThenInclude(oi => oi.InventoryItem) // Eagerly load InventoryItem for each OrderItem
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                _logger.LogWarning("No orders found for tenant ID: {TenantId}.", tenantId);
            }

            return orders;
        }



    }
}
