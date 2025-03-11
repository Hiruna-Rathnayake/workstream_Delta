using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.Model;
using workstream.DTO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using workstream.Services;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderRepo _orderRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;
        private readonly JwtService _jwtService;

        public OrderController(OrderRepo orderRepo, IMapper mapper, ILogger<OrderController> logger, JwtService jwtService)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            // Get tenantId from token using the JwtService
            var tokenTenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));

            if (tokenTenantId <= 0)
            {
                _logger.LogWarning("Invalid or unauthorized tenant ID.");
                return Unauthorized("Invalid token or tenant ID.");
            }

            _logger.LogInformation("Fetching all orders for tenant ID: {TenantId}.", tokenTenantId);

            try
            {
                var orders = await _orderRepo.GetAllOrdersByTenantIdAsync(tokenTenantId);
                var orderReadDtos = _mapper.Map<List<OrderReadDTO>>(orders);

                if (orders == null || orders.Count == 0)
                {
                    return NotFound($"No orders found for tenant ID: {tokenTenantId}");
                }

                return Ok(orderReadDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching orders for tenant {tokenTenantId}: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }


        // Create a new Order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderWriteDTO orderWriteDto)
        {
            if (orderWriteDto == null)
            {
                _logger.LogError("OrderWriteDTO cannot be null.");
                return BadRequest("Order data cannot be null.");
            }

            // Get tenantId from token
            var tenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
            _logger.LogInformation("Creating order for tenant {TenantId}.", tenantId);

            var order = _mapper.Map<Order>(orderWriteDto);
            // Ensure OrderItems is populated from ItemsToAdd
            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }
            if (orderWriteDto.ItemsToAdd != null)
            {
                foreach (var item in orderWriteDto.ItemsToAdd)
                {
                    order.OrderItems.Add(_mapper.Map<OrderItem>(item));
                }
            }

            try
            {
                var createdOrder = await _orderRepo.CreateOrderAsync(order, tenantId);
                var orderReadDto = _mapper.Map<OrderReadDTO>(createdOrder);

                return CreatedAtAction(nameof(GetOrderById), new { orderId = orderReadDto.OrderId, tenantId }, orderReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Get Order by ID
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            // Get tenantId from token
            var tenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
            _logger.LogInformation("Fetching order {OrderId} for tenant {TenantId}.", orderId, tenantId);

            try
            {
                var order = await _orderRepo.GetOrderByIdAsync(orderId, tenantId);
                var orderReadDto = _mapper.Map<OrderReadDTO>(order);

                return Ok(orderReadDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching order: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Update an Order (Add/Remove items, change status)
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] OrderWriteDTO orderWriteDto)
        {
            if (orderWriteDto == null)
            {
                _logger.LogError("OrderWriteDTO cannot be null.");
                return BadRequest("Order data cannot be null.");
            }

            // Get tenantId from token
            var tenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
            _logger.LogInformation("Updating order {OrderId} for tenant {TenantId}.", orderId, tenantId);

            try
            {
                var updatedOrder = await _orderRepo.UpdateOrderAsync(orderId, tenantId, orderWriteDto);
                var orderReadDto = _mapper.Map<OrderReadDTO>(updatedOrder);

                return Ok(orderReadDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Add Inventory Item to an existing Order
        [HttpPost("{orderId}/items")]
        public async Task<IActionResult> AddInventoryItemToOrder(int orderId, [FromBody] OrderItemWriteDTO orderItemWriteDto)
        {
            if (orderItemWriteDto == null)
            {
                _logger.LogError("OrderItemWriteDTO cannot be null.");
                return BadRequest("Order item data cannot be null.");
            }

            // Get tenantId from token
            var tenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
            _logger.LogInformation("Adding item to order {OrderId} for tenant {TenantId}.", orderId, tenantId);

            try
            {
                var orderItem = await _orderRepo.AddInventoryItemToOrderAsync(orderId, orderItemWriteDto.InventoryItemId, orderItemWriteDto.Quantity, tenantId);
                var orderItemReadDto = _mapper.Map<OrderItemReadDTO>(orderItem);

                return CreatedAtAction(nameof(GetOrderItems), new { orderId, tenantId }, orderItemReadDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding inventory item to order: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Get all OrderItems for an Order
        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            // Get tenantId from token
            var tenantId = _jwtService.GetTenantIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
            _logger.LogInformation("Fetching items for order {OrderId} for tenant {TenantId}.", orderId, tenantId);

            try
            {
                var orderItems = await _orderRepo.GetOrderItemsAsync(orderId, tenantId);
                var orderItemReadDtos = _mapper.Map<List<OrderItemReadDTO>>(orderItems);

                return Ok(orderItemReadDtos);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching order items: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
