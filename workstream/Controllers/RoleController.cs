using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.Model;
using workstream.DTO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using workstream.Services;
using Microsoft.AspNetCore.Authorization;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleRepo _roleRepo;
        private readonly PermissionRepo _permissionRepo;
        private readonly ILogger<RoleController> _logger;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;

        public RoleController(
            RoleRepo roleRepo,
            PermissionRepo permissionRepo,
            ILogger<RoleController> logger,
            IMapper mapper,
            JwtService jwtService)
        {
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _logger = logger;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRolesAsync()
        {
            try
            {
                // Get the token from the Authorization header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Get TenantId from the token using JwtService
                var tenantId = _jwtService.GetTenantIdFromToken(token);

                // Get roles filtered by TenantId
                var roles = await _roleRepo.GetAllRolesAsync(tenantId);

                // Map roles to DTOs if necessary
                var roleReadDTOs = _mapper.Map<IEnumerable<RoleReadDTO>>(roles);

                return Ok(roleReadDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("with-permissions")]
        public async Task<IActionResult> GetRolesWithPermissionsAsync()
        {
            try
            {
                // Get the token from the Authorization header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Get TenantId from the token using JwtService
                var tenantId = _jwtService.GetTenantIdFromToken(token);

                // Log fetching roles with permissions for the given tenantId
                _logger.LogInformation("\x1b[33mFetching roles with permissions for tenant {TenantId}.\x1b[0m", tenantId);

                // Get roles with permissions filtered by TenantId
                var roles = await _roleRepo.GetRolesWithPermissionsAsync(tenantId);

                // Map roles to DTOs if necessary
                var roleReadDTOs = _mapper.Map<IEnumerable<RoleWithPermissionsDTO>>(roles);

                // Log the number of roles found
                _logger.LogInformation("\x1b[33mFound {RoleCount} roles with permissions for tenant {TenantId}.\x1b[0m", roles.Count, tenantId);

                return Ok(roleReadDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/roles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleWithPermissionsDTO>> GetRoleByIdAsync(int id)
        {
            try
            {
                // Fetch the role with its permissions
                var role = await _roleRepo.GetRoleByIdAsync(id);

                // If role is not found, return a NotFound response
                if (role == null)
                {
                    return NotFound($"Role with ID {id} not found.");
                }

                // Map role to RoleWithPermissionsDTO directly using AutoMapper
                var roleDto = _mapper.Map<RoleWithPermissionsDTO>(role);

                // Return the DTO with role and permissions
                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                // Log the error and return internal server error
                _logger.LogError("Error fetching role with ID {RoleId}: {Message}", id, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }



        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RoleReadDTO>> CreateRoleAsync([FromBody] RoleWriteDTO roleWriteDto)
        {
            try
            {
                if (roleWriteDto == null)
                {
                    _logger.LogWarning("Received null role data.");
                    return BadRequest("Role data is required.");
                }

                // Extract TenantId from the token
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tenantId = _jwtService.GetTenantIdFromToken(token);
                if (tenantId == 0)
                {
                    return Unauthorized("Invalid or missing tenant information.");
                }

                var role = _mapper.Map<Role>(roleWriteDto);
                role.TenantId = tenantId; // Ensure role is linked to the correct tenant

                var createdRole = await _roleRepo.CreateRoleAsync(role, roleWriteDto.PermissionNames);

                var roleDto = _mapper.Map<RoleReadDTO>(createdRole);
                return CreatedAtAction(nameof(GetRoleByIdAsync), new { id = roleDto.RoleId }, roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating role: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }


        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleReadDTO>> UpdateRoleAsync(int id, [FromBody] RoleWriteDTO roleWriteDto)
        {
            try
            {
                if (roleWriteDto == null)
                {
                    _logger.LogWarning("Received null role data.");
                    return BadRequest("Role data is required.");
                }

                var updatedRole = await _roleRepo.UpdateRoleAsync(id, roleWriteDto.Name, roleWriteDto.PermissionNames);
                var roleDto = _mapper.Map<RoleReadDTO>(updatedRole);

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating role with ID {RoleId}: {Message}", id, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoleAsync(int id)
        {
            try
            {
                var success = await _roleRepo.DeleteRoleAsync(id);
                if (!success)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found.", id);
                    return NotFound($"Role with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting role with ID {RoleId}: {Message}", id, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

