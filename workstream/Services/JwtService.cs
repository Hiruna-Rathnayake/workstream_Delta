using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using workstream.Model;
using System.Threading.Tasks;
using workstream.Data; // Assuming PermissionRepo is in this namespace

namespace workstream.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly PermissionRepo _permissionRepo;  // Inject PermissionRepo

        public JwtService(string secretKey, string issuer, string audience, PermissionRepo permissionRepo)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _permissionRepo = permissionRepo;  // Initialize PermissionRepo
        }

        // Generate a JWT token
        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Guest"),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("RoleId", user.RoleId.ToString())  // Add a separate claim for RoleId

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Validate the token and extract user info
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false // We don't want to validate the token's expiration at this point
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out var securityToken);

                if (securityToken is JwtSecurityToken jwtToken)
                {
                    if (jwtToken.SignatureAlgorithm != SecurityAlgorithms.HmacSha256)
                    {
                        throw new SecurityTokenException("Invalid token algorithm");
                    }
                }

                return principal;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token", ex);
            }
        }

        // Get TenantId from token
        public int GetTenantIdFromToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var tenantIdClaim = principal?.FindFirst("TenantId");

            if (tenantIdClaim == null)
            {
                throw new UnauthorizedAccessException("TenantId claim is missing in token.");
            }

            return int.Parse(tenantIdClaim.Value);
        }

        // Get RoleId from token
        public int GetRoleIdFromToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var roleIdClaim = principal?.FindFirst("RoleId");

            if (roleIdClaim == null)
            {
                throw new UnauthorizedAccessException("RoleId claim is missing in token.");
            }

            return int.Parse(roleIdClaim.Value);
        }


        // Check if user has a specific permission
        public async Task<bool> UserHasPermissionAsync(string token, string requiredPermission)
        {
            var roleId = GetRoleIdFromToken(token); // Get RoleId from the new claim
            var tenantId = GetTenantIdFromToken(token); // Get TenantId

            return await _permissionRepo.DoesRoleHavePermissionAsync(roleId, tenantId, requiredPermission);
        }

    }
}
