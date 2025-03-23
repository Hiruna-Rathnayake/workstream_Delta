using workstream.Data;
using workstream.Model;
using Microsoft.EntityFrameworkCore;
using workstream.DTO;

public class TenantRepo
{
    private readonly WorkstreamDbContext _context;

    public TenantRepo(WorkstreamDbContext context)
    {
        _context = context;
    }

    // Create a new Tenant, associate a User with default role, and ensure basic functionality is tested first
    public async Task<Tenant> CreateTenantWithUserAsync(TenantWriteDTO tenantDTO, UserWriteDTO userDTO)
    {
        if (tenantDTO == null || userDTO == null)
        {
            throw new ArgumentNullException("Tenant or User data cannot be null.");
        }

        try
        {
            // Step 1: Create Tenant
            var tenant = new Tenant
            {
                CompanyName = tenantDTO.CompanyName,
                ContactEmail = tenantDTO.ContactEmail
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();  // Save Tenant and generate TenantId

            // Step 2: Create Default Roles
            try
            {
                var roles = new List<Role>
            {
                new Role { Name = "Owner", TenantId = tenant.TenantId },
                new Role { Name = "Admin", TenantId = tenant.TenantId },
                new Role { Name = "Manager", TenantId = tenant.TenantId },
                new Role { Name = "User", TenantId = tenant.TenantId },
                new Role { Name = "Guest", TenantId = tenant.TenantId }
            };

                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();  // Save roles

                // Fetch all existing permissions
                var permissions = await _context.Permissions.ToListAsync();
                if (!permissions.Any())
                {
                    throw new InvalidOperationException("No default permissions found in the database.");
                }

                // Assign all permissions to the "Owner" role (full access)
                var ownerRole = roles.FirstOrDefault(r => r.Name == "Owner");
                if (ownerRole == null)
                {
                    throw new InvalidOperationException("Owner role not found in roles list.");
                }

                var rolePermissions = permissions.Select(p => new RolePermission
                {
                    RoleId = ownerRole.RoleId,
                    TenantId = tenant.TenantId,
                    PermissionId = p.PermissionId
                }).ToList();

                _context.RolePermissions.AddRange(rolePermissions);
                await _context.SaveChangesAsync(); // Save role-permission assignments

                // Step 4: Create User and assign the "Owner" role
                var user = new User
                {
                    Username = userDTO.Username,
                    PasswordHash = userDTO.PasswordHash,  // Ensure password is hashed
                    TenantId = tenant.TenantId,  // Set the TenantId after tenant is created
                    RoleId = ownerRole.RoleId  // Assign the "Owner" role from the list
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();  // Save user

                return tenant;
            }
            catch (Exception roleCreationEx)
            {
                // If role or permission assignment fails, clean up tenant
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();  // Rollback tenant creation

                throw new InvalidOperationException("Error occurred while creating roles or assigning permissions.", roleCreationEx);
            }
        }
        catch (Exception tenantCreationEx)
        {
            // Handle tenant creation failure
            throw new InvalidOperationException("Error occurred while creating tenant.", tenantCreationEx);
        }
    }



    // Read a single Tenant by ID
    public async Task<Tenant> GetTenantByIdAsync(int tenantId)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));

        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (tenant == null)
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        return tenant;
    }

    // Update an existing Tenant
    public async Task<Tenant> UpdateTenantAsync(int tenantId, Tenant updatedTenant)
    {
        if (updatedTenant == null)
            throw new ArgumentNullException(nameof(updatedTenant), "Updated tenant data cannot be null.");

        var tenant = await GetTenantByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
        }

        tenant.CompanyName = updatedTenant.CompanyName ?? tenant.CompanyName;
        tenant.ContactEmail = updatedTenant.ContactEmail ?? tenant.ContactEmail;

        try
        {
            await _context.SaveChangesAsync();
            return tenant;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            throw new InvalidOperationException("Error occurred while updating tenant.", ex);
        }
    }

    // Delete a Tenant
    public async Task<bool> DeleteTenantAsync(int tenantId)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));

        var tenant = await _context.Tenants
            .Include(t => t.Users)
            .Include(t => t.Roles)
                .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
        }

        try
        {
            // Remove all Users under this Tenant
            _context.Users.RemoveRange(tenant.Users);

            // Remove all Roles and their associated RolePermissions
            foreach (var role in tenant.Roles)
            {
                _context.RolePermissions.RemoveRange(role.RolePermissions);
            }
            _context.Roles.RemoveRange(tenant.Roles);

            // Finally, remove the Tenant
            _context.Tenants.Remove(tenant);

            // Save all changes
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            throw new InvalidOperationException("Error occurred while deleting tenant.", ex);
        }
    }
}
