using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using workstream.Model;
using workstream.DTO;

namespace workstream.Data
{
    public class RoleRepo
    {
        private readonly WorkstreamDbContext _context;
        private readonly ILogger<RoleRepo> _logger;

        public RoleRepo(WorkstreamDbContext context, ILogger<RoleRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create a new Role and assign permissions by name
        public async Task<Role> CreateRoleAsync(Role role, List<string> permissionNames)
        {
            if (role == null)
            {
                _logger.LogError("Role data cannot be null.");
                throw new ArgumentNullException(nameof(role), "Role data cannot be null.");
            }

            // Ensure the Tenant exists
            var tenantExists = await _context.Tenants
                .AnyAsync(t => t.TenantId == role.TenantId);

            if (!tenantExists)
            {
                _logger.LogError("Tenant with ID: {TenantId} not found.", role.TenantId);
                throw new KeyNotFoundException($"Tenant with ID {role.TenantId} not found.");
            }

            // Add the new role and save it first to generate RoleId
            _logger.LogInformation("Creating role: {RoleName}.", role.Name);
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(); // Ensure RoleId is generated
            _logger.LogInformation("Role created with RoleId: {RoleId}.", role.RoleId);

            if (permissionNames != null && permissionNames.Any())
            {
                _logger.LogInformation("Checking permissions: {Permissions}", string.Join(", ", permissionNames));

                // Fetch only EXISTING permissions
                var permissions = new List<Permission>();
                foreach (var permName in permissionNames)
                {
                    var permission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.Name == permName);

                    if (permission == null)
                    {
                        _logger.LogWarning("Permission with name '{PermissionName}' does not exist.", permName);
                    }
                    else
                    {
                        _logger.LogInformation("Permission found: {PermissionName}, PermissionId: {PermissionId}", permission.Name, permission.PermissionId);
                        permissions.Add(permission);
                    }
                }

                // If permissions are not found for all the names, log and throw
                if (permissions.Count != permissionNames.Count)
                {
                    _logger.LogError("One or more permissions not found for the role.");
                    throw new KeyNotFoundException("One or more permissions not found.");
                }

                // Check for existing RolePermissions before adding new ones
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == role.RoleId)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                _logger.LogInformation("Existing RolePermissions for RoleId {RoleId}: {ExistingPermissions}", role.RoleId, string.Join(", ", existingRolePermissions));

                // Get only the permissions that are not already assigned to the role
                var newRolePermissions = permissions
                    .Where(p => !existingRolePermissions.Contains(p.PermissionId)) // Prevent duplicates
                    .Select(p => new RolePermission
                    {
                        RoleId = role.RoleId,
                        TenantId = role.TenantId,
                        PermissionId = p.PermissionId
                    })
                    .ToList();

                _logger.LogInformation("New RolePermissions to be added: {NewPermissions}", string.Join(", ", newRolePermissions.Select(rp => rp.PermissionId)));

                if (newRolePermissions.Any()) // Only save if there's something new
                {
                    _context.RolePermissions.AddRange(newRolePermissions);
                    // Ensure EF knows these are existing permissions
                    foreach (var rolePermission in newRolePermissions)
                    {
                        _context.Entry(rolePermission).Property(x => x.PermissionId).IsModified = true;
                        _context.Entry(rolePermission).Reference(x => x.Permission).IsModified = false;
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("{Count} new RolePermissions added for RoleId {RoleId}.", newRolePermissions.Count, role.RoleId);
                }
                else
                {
                    _logger.LogInformation("No new RolePermissions to add for RoleId {RoleId} as all permissions are already assigned.", role.RoleId);
                }
            }
            else
            {
                _logger.LogInformation("No permissions to add for RoleId {RoleId}.", role.RoleId);
            }

            _logger.LogInformation("Role '{RoleName}' created successfully with permissions.", role.Name);

            return role;
        }




        public async Task<Role> UpdateRoleAsync(int roleId, string roleName, List<string> permissionNames)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                _logger.LogError("DEBUG MODE: Role name cannot be null or empty.");
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
            }

            _logger.LogInformation("DEBUG MODE: Updating role with ID: {RoleId}.", roleId);

            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
            {
                _logger.LogWarning("DEBUG MODE: Role with ID: {RoleId} not found.", roleId);
                throw new KeyNotFoundException($"Role with ID {roleId} not found.");
            }

            // Update role name
            role.Name = roleName;
            _context.Entry(role).State = EntityState.Modified;

            // Remove existing RolePermissions
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingRolePermissions);
            _logger.LogInformation("DEBUG MODE: Removed {Count} existing permissions for Role ID: {RoleId}.", existingRolePermissions.Count, roleId);

            // Add new RolePermissions based on the provided names
            if (permissionNames?.Any() == true)
            {
                var permissions = await _context.Permissions
                    .Where(p => permissionNames.Contains(p.Name))
                    .ToListAsync();

                if (permissions.Count != permissionNames.Count)
                {
                    var missingPermissions = permissionNames.Except(permissions.Select(p => p.Name)).ToList();
                    _logger.LogError("DEBUG MODE: Some permissions not found! Missing: {Missing}", string.Join(", ", missingPermissions));
                    throw new KeyNotFoundException("One or more permissions not found.");
                }

                var newRolePermissions = permissions.Select(p => new RolePermission
                {
                    RoleId = role.RoleId,
                    TenantId = role.TenantId,
                    PermissionId = p.PermissionId
                }).ToList();

                _context.RolePermissions.AddRange(newRolePermissions);
                _logger.LogInformation("DEBUG MODE: Added {Count} new permissions for Role ID: {RoleId}.", newRolePermissions.Count, roleId);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("DEBUG MODE: Role with ID: {RoleId} updated successfully.", roleId);

            return role;
        }






        // Delete Role
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            if (roleId <= 0)
            {
                _logger.LogError("Invalid role ID: {RoleId}.", roleId);
                throw new ArgumentException("Invalid role ID.", nameof(roleId));
            }

            _logger.LogInformation("Deleting role with ID: {RoleId}.", roleId);

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
            {
                _logger.LogWarning("Role with ID: {RoleId} not found.", roleId);
                throw new KeyNotFoundException($"Role with ID {roleId} not found.");
            }

            // Remove the role's permissions association (but don't delete permissions themselves)
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);

            // Remove the role itself
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role with ID: {RoleId} deleted successfully.", roleId);
            return true;
        }

        public async Task<List<Role>> GetAllRolesAsync(int tenantId)
        {
            _logger.LogInformation("Fetching roles for tenant {TenantId}.", tenantId);

            var roles = await _context.Roles
                .Where(r => r.TenantId == tenantId) // Filter roles by tenantId
                .ToListAsync(); // Fetch roles

            _logger.LogInformation("Found {RoleCount} roles for tenant {TenantId}.", roles.Count, tenantId);

            return roles;
        }


        public async Task<List<RoleWithPermissionsDTO>> GetRolesWithPermissionsAsync(int tenantId)
        {
            _logger.LogInformation("\x1b[33mFetching roles with permissions for tenant {TenantId}.\x1b[0m", tenantId); // Yellow log

            var roles = await _context.Roles
                .Where(r => r.TenantId == tenantId) // Filter roles by tenantId
                .Include(r => r.RolePermissions)  // Include the associated RolePermissions
                .ThenInclude(rp => rp.Permission) // Include the related Permission for each RolePermission
                .ToListAsync(); // Fetch roles and their associated permissions

            // Transform the data to include permission names in a DTO
            var rolesWithPermissions = roles.Select(role => new RoleWithPermissionsDTO
            {
                RoleId = role.RoleId,
                Name = role.Name,
                PermissionNames = role.RolePermissions.Select(rp => rp.Permission.Name).ToList() // Collect permission names
            }).ToList();

            _logger.LogInformation("\x1b[33mFound {RoleCount} roles with permissions for tenant {TenantId}.\x1b[0m", roles.Count, tenantId); // Yellow log

            return rolesWithPermissions;
        }




        // Get Role by ID
        public async Task<RoleWithPermissionsDTO> GetRoleByIdAsync(int roleId)
        {
            if (roleId <= 0)
            {
                _logger.LogError("Invalid role ID: {RoleId}.", roleId);
                throw new ArgumentException("Invalid role ID.", nameof(roleId));
            }

            _logger.LogInformation("Fetching role with ID: {RoleId} and its permissions.", roleId);

            var role = await _context.Roles
                .Where(r => r.RoleId == roleId) // Filter by roleId
                .Include(r => r.RolePermissions) // Include associated RolePermissions
                .ThenInclude(rp => rp.Permission) // Include related Permission for each RolePermission
                .FirstOrDefaultAsync(); // Fetch the role and its permissions

            if (role == null)
            {
                _logger.LogWarning("Role with ID: {RoleId} not found.", roleId);
                throw new KeyNotFoundException($"Role with ID {roleId} not found.");
            }

            // Map Role entity to RoleWithPermissionsDTO, including permissions
            var roleWithPermissionsDto = new RoleWithPermissionsDTO
            {
                RoleId = role.RoleId,
                Name = role.Name,
                PermissionNames = role.RolePermissions.Select(rp => rp.Permission.Name).ToList()
            };

            return roleWithPermissionsDto; // Return the DTO with permissions
        }


    }
}
