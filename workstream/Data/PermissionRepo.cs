using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using workstream.Model;

namespace workstream.Data
{
    public class PermissionRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<PermissionRepo> _logger;

        public PermissionRepo(WorkstreamDbContext context, ILogger<PermissionRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Check if a role has a specific permission
        public async Task<bool> DoesRoleHavePermissionAsync(int roleId, int tenantId, string permissionName)
        {
            _logger.LogInformation("Checking if role {RoleId} has permission: {PermissionName} for tenant {TenantId}.", roleId, permissionName, tenantId);

            var hasPermission = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .Join(
                    _context.Permissions,  // Join with Permissions table
                    rp => rp.PermissionId,  // Matching RolePermission.PermissionId 
                    p => p.PermissionId,    // With Permission.PermissionId
                    (rp, p) => p.Name       // Select the permission name
                )
                .AnyAsync(pName => pName == permissionName); // Check if the permission exists

            if (hasPermission)
            {
                _logger.LogInformation("Role {RoleId} has the permission: {PermissionName}.", roleId, permissionName);
                return true;
            }

            _logger.LogWarning("Role {RoleId} does NOT have the permission: {PermissionName}.", roleId, permissionName);
            return false;
        }


        // Get Permission by ID
        public async Task<Permission> GetPermissionByIdAsync(int permissionId)
        {
            if (permissionId <= 0)
            {
                _logger.LogError("Invalid permission ID: {PermissionId}.", permissionId);
                throw new ArgumentException("Invalid permission ID.", nameof(permissionId));
            }

            _logger.LogInformation("Fetching permission with ID: {PermissionId}.", permissionId);

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId);

            if (permission == null)
            {
                _logger.LogWarning("Permission with ID: {PermissionId} not found.", permissionId);
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found.");
            }

            return permission;
        }

        // Get all Permissions (available to all tenants)
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            _logger.LogInformation("Fetching all system permissions.");

            var permissions = await _context.Permissions
                .ToListAsync(); // No filtering by tenant, as permissions are global

            _logger.LogInformation("Found {PermissionCount} system permissions.", permissions.Count);

            return permissions;
        }

        // Get Permission by Name (useful when you know the permission name, e.g., "Admin", "Manager")
        public async Task<Permission> GetPermissionByNameAsync(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                _logger.LogError("Permission name cannot be null or empty.");
                throw new ArgumentException("Permission name cannot be null or empty.", nameof(permissionName));
            }

            _logger.LogInformation("Fetching permission with name: {PermissionName}.", permissionName);

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionName);

            if (permission == null)
            {
                _logger.LogWarning("Permission with name: {PermissionName} not found.", permissionName);
                throw new KeyNotFoundException($"Permission with name {permissionName} not found.");
            }

            return permission;
        }
    }
}
