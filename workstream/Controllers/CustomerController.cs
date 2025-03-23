using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.DTO;
using workstream.Model;
using AutoMapper;
using Microsoft.Extensions.Logging;
using workstream.Services;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepo _customerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerController> _logger;
        private readonly JwtService _jwtService; // Add JwtService to extract Tenant ID

        public CustomerController(CustomerRepo customerRepo, IMapper mapper, ILogger<CustomerController> logger, JwtService jwtService)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
            _logger = logger;
            _jwtService = jwtService; // Inject JwtService
        }

        // Create a new Customer
        [HttpPost]
        public async Task<ActionResult<CustomerReadDTO>> CreateCustomer([FromBody] CustomerWriteDTO customerWriteDTO)
        {
            if (customerWriteDTO == null)
            {
                _logger.LogError("Received null data for customer creation.");
                return BadRequest("Customer data cannot be null.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _jwtService.UserHasPermissionAsync(token, "CustomerManagement"))
            {
                return Forbid("Insufficient permissions.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token); // Extract tenantId from token
            _logger.LogInformation("Creating customer for tenant {TenantId}.", tenantId);

            var customer = _mapper.Map<Customer>(customerWriteDTO); // Mapping DTO to Model
            customer.TenantId = tenantId;

            var createdCustomer = await _customerRepo.CreateCustomerAsync(customer); // Use only the customer object, no need for tenantId

            var customerReadDTO = _mapper.Map<CustomerReadDTO>(createdCustomer); // Mapping back to DTO for response
            _logger.LogInformation("Customer created for tenant {TenantId}, CustomerId: {CustomerId}.", tenantId, createdCustomer.CustomerId);
            return CreatedAtAction(nameof(GetCustomerById), new { customerId = createdCustomer.CustomerId }, customerReadDTO);
        }

        // Get Customer by ID
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerReadDTO>> GetCustomerById(int customerId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _jwtService.UserHasPermissionAsync(token, "CustomerManagement"))
            {
                return Forbid("Insufficient permissions.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token); // Extract tenantId from token
            _logger.LogInformation("Fetching customer {CustomerId} for tenant {TenantId}.", customerId, tenantId);

            try
            {
                var customer = await _customerRepo.GetCustomerByIdAsync(customerId, tenantId);
                var customerReadDTO = _mapper.Map<CustomerReadDTO>(customer);
                _logger.LogInformation("Customer {CustomerId} retrieved for tenant {TenantId}.", customerId, tenantId);
                return Ok(customerReadDTO);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Customer {CustomerId} not found for tenant {TenantId}. Exception: {ExceptionMessage}", customerId, tenantId, ex.Message);
                return NotFound($"Customer with ID {customerId} not found.");
            }
        }

        // Get all Customers for a Tenant
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerReadDTO>>> GetAllCustomers()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _jwtService.UserHasPermissionAsync(token, "CustomerManagement"))
            {
                return Forbid("Insufficient permissions.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token); // Extract tenantId from token
            _logger.LogInformation("Fetching all customers for tenant {TenantId}.", tenantId);

            try
            {
                var customers = await _customerRepo.GetAllCustomersAsync(tenantId);
                var customersReadDTO = _mapper.Map<List<CustomerReadDTO>>(customers);
                _logger.LogInformation("Found {CustomerCount} customers for tenant {TenantId}.", customersReadDTO.Count, tenantId);
                return Ok(customersReadDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching customers for tenant {TenantId}. Exception: {ExceptionMessage}", tenantId, ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        // Soft delete a Customer
        [HttpDelete("{customerId}")]
        public async Task<ActionResult> SoftDeleteCustomer(int customerId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _jwtService.UserHasPermissionAsync(token, "CustomerManagement"))
            {
                return Forbid("Insufficient permissions.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token); // Extract tenantId from token
            _logger.LogInformation("Deleting customer {CustomerId} for tenant {TenantId}.", customerId, tenantId);

            try
            {
                var success = await _customerRepo.SoftDeleteCustomerAsync(customerId, tenantId);
                if (success)
                {
                    _logger.LogInformation("Customer {CustomerId} deleted for tenant {TenantId}.", customerId, tenantId);
                    return NoContent(); // 204 No Content (Successfully deleted)
                }
                return NotFound($"Customer with ID {customerId} not found.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Customer {CustomerId} not found for tenant {TenantId}. Exception: {ExceptionMessage}", customerId, tenantId, ex.Message);
                return NotFound($"Customer with ID {customerId} not found.");
            }
        }

        // Update a Customer
        [HttpPut("{customerId}")]
        public async Task<ActionResult<CustomerReadDTO>> UpdateCustomer(int customerId, [FromBody] CustomerWriteDTO customerWriteDTO)
        {
            if (customerWriteDTO == null)
            {
                _logger.LogError("Received null data for customer update.");
                return BadRequest("Customer data cannot be null.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _jwtService.UserHasPermissionAsync(token, "CustomerManagement"))
            {
                return Forbid("Insufficient permissions.");
            }

            var tenantId = _jwtService.GetTenantIdFromToken(token); // Extract tenantId from token
            _logger.LogInformation("Updating customer {CustomerId} for tenant {TenantId}.", customerId, tenantId);

            try
            {
                var customerToUpdate = _mapper.Map<Customer>(customerWriteDTO); // Mapping DTO to Model
                var updatedCustomer = await _customerRepo.UpdateCustomerAsync(customerId, customerToUpdate, tenantId);
                var customerReadDTO = _mapper.Map<CustomerReadDTO>(updatedCustomer); // Mapping back to DTO for response

                _logger.LogInformation("Customer {CustomerId} updated for tenant {TenantId}.", customerId, tenantId);
                return Ok(customerReadDTO);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Customer {CustomerId} not found for tenant {TenantId}. Exception: {ExceptionMessage}", customerId, tenantId, ex.Message);
                return NotFound($"Customer with ID {customerId} not found.");
            }
        }
    }
}
