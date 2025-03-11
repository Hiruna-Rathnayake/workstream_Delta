using Microsoft.EntityFrameworkCore;
using workstream.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace workstream.Data
{
    public class UserRepo
    {
        private readonly WorkstreamDbContext _context;

        public UserRepo(WorkstreamDbContext context)
        {
            _context = context;
        }

        // Create a new User
        public async Task CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while creating user.", ex);
            }
        }

        // Get all Users for a specific Tenant
        public async Task<IEnumerable<User>> GetAllUsersAsync(int tenantId)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.TenantId == tenantId) // Filter by TenantId
                    .Include(u => u.Role) // Include the Role navigation property if needed
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while retrieving users.", ex);
            }
        }


        // Read a User by ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            var user = await _context.Users
                .Include(u => u.Role) // Include the Role navigation property
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            return user;
        }

        // Update an existing User
        public async Task<User> UpdateUserAsync(int userId, User updatedUser)
        {
            if (updatedUser == null)
                throw new ArgumentNullException(nameof(updatedUser), "Updated user data cannot be null.");

            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            user.Username = updatedUser.Username ?? user.Username;
            user.PasswordHash = updatedUser.PasswordHash ?? user.PasswordHash;

            // Ensure RoleId is only updated if it's not null
            if (updatedUser.RoleId.HasValue)
            {
                user.RoleId = updatedUser.RoleId.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while updating user.", ex);
            }
        }

        // Read a User by Username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            var user = await _context.Users
                .Include(u => u.Role) // Include the Role navigation property
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                throw new KeyNotFoundException($"User with username {username} not found.");

            return user;
        }

        // Delete a User
        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while deleting user.", ex);
            }
        }
    }
}
