using Microsoft.EntityFrameworkCore;
using workstream.Model;

namespace workstream.Data
{
    public class CustomerRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<CustomerRepo> _logger;

        public CustomerRepo(WorkstreamDbContext context, ILogger<CustomerRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Customer
        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                _logger.LogError("Customer data cannot be null.");
                throw new ArgumentNullException(nameof(customer), "Customer data cannot be null.");
            }

            _logger.LogInformation("Creating customer with name: {Name}", customer.Name);

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer with ID: {CustomerId} created successfully.", customer.CustomerId);

            return customer;
        }

        // Get Customer by ID (excluding soft-deleted ones)
        public async Task<Customer> GetCustomerByIdAsync(int customerId, int tenantId)
        {
            if (customerId <= 0)
            {
                _logger.LogError("Invalid customer ID.");
                throw new ArgumentException("Invalid customer ID.", nameof(customerId));
            }

            _logger.LogInformation("Fetching customer with ID: {CustomerId} for tenant ID: {TenantId}", customerId, tenantId);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.TenantId == tenantId && !c.IsDeleted);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID: {CustomerId} not found for tenant ID: {TenantId}.", customerId, tenantId);
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }

            return customer;
        }

        // Get all Customers for a Tenant (excluding soft-deleted ones)
        public async Task<List<Customer>> GetAllCustomersAsync(int tenantId)
        {
            _logger.LogInformation("Fetching all customers for tenant ID: {TenantId}", tenantId);

            var customers = await _context.Customers
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .ToListAsync();

            _logger.LogInformation("Found {CustomerCount} customers for tenant ID: {TenantId}", customers.Count, tenantId);

            return customers;
        }

        // Soft delete a Customer
        public async Task<bool> SoftDeleteCustomerAsync(int customerId, int tenantId)
        {
            _logger.LogInformation("Soft deleting customer with ID: {CustomerId} for tenant ID: {TenantId}", customerId, tenantId);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.TenantId == tenantId && !c.IsDeleted);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID: {CustomerId} not found for tenant ID: {TenantId}.", customerId, tenantId);
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }

            customer.IsDeleted = true; // Mark as deleted
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer with ID: {CustomerId} soft deleted successfully for tenant ID: {TenantId}", customerId, tenantId);

            return true;
        }

        // Update a Customer
        public async Task<Customer> UpdateCustomerAsync(int customerId, Customer updatedCustomer, int tenantId)
        {
            if (updatedCustomer == null)
            {
                _logger.LogError("Updated customer data cannot be null.");
                throw new ArgumentNullException(nameof(updatedCustomer), "Updated customer data cannot be null.");
            }

            _logger.LogInformation("Updating customer with ID: {CustomerId} for tenant ID: {TenantId}", customerId, tenantId);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.TenantId == tenantId && !c.IsDeleted);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID: {CustomerId} not found for tenant ID: {TenantId}.", customerId, tenantId);
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }

            customer.Name = updatedCustomer.Name ?? customer.Name;
            customer.Email = updatedCustomer.Email ?? customer.Email;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer with ID: {CustomerId} updated successfully for tenant ID: {TenantId}", customerId, tenantId);

            return customer;
        }
    }
}
