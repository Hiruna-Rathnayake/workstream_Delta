using Microsoft.AspNetCore.Mvc;
using workstream.Model;
using workstream.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using workstream.DTO;
using AutoMapper;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly TenantRepo _tenantRepo;
        private readonly IMapper _mapper;

        public TenantController(TenantRepo tenantRepo, IMapper mapper)
        {
            _tenantRepo = tenantRepo;
            _mapper = mapper;
        }

        // POST: api/Tenant/CreateTenantWithUser
        [HttpPost("CreateTenantWithUser")]
        public async Task<IActionResult> CreateTenantWithUser([FromBody] TenantWithUserDTO tenantWithUserDTO)
        {
            if (tenantWithUserDTO == null || tenantWithUserDTO.Tenant == null || tenantWithUserDTO.User == null)
            {
                return BadRequest("Tenant or User data cannot be null.");
            }

            try
            {
                // Step 1: Perform tenant and user creation
                var tenant = await _tenantRepo.CreateTenantWithUserAsync(tenantWithUserDTO.Tenant, tenantWithUserDTO.User);

                // Step 2: Return a created response with the tenant
                return CreatedAtAction(nameof(GetTenantById), new { tenantId = tenant.TenantId }, tenant);
            }
            catch (ArgumentNullException argEx)
            {
                // Specific catch for null argument errors (like null data)
                return BadRequest($"Invalid input data: {argEx.Message}");
            }
            catch (InvalidOperationException invOpEx)
            {
                // Catch specific exceptions like invalid operations
                return BadRequest($"Invalid operation: {invOpEx.Message}");
            }
            catch (Exception ex)
            {
                // Generic catch for unexpected errors
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while creating tenant and user: {ex.Message}");
            }
        }


        // GET: api/Tenant/5
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenantById(int tenantId)
        {
            try
            {
                var tenant = await _tenantRepo.GetTenantByIdAsync(tenantId);
                var tenantDTO = _mapper.Map<TenantReadDTO>(tenant);  // AutoMapper for reading tenant
                return Ok(tenantDTO);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Tenant with ID {tenantId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while retrieving tenant: {ex.Message}");
            }
        }

        // PUT: api/Tenant/5
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(int tenantId, [FromBody] TenantWriteDTO updatedTenantDTO)
        {
            if (updatedTenantDTO == null)
            {
                return BadRequest("Updated tenant data cannot be null.");
            }

            try
            {
                var updatedTenant = _mapper.Map<Tenant>(updatedTenantDTO);  // AutoMapper for updating tenant
                var tenant = await _tenantRepo.UpdateTenantAsync(tenantId, updatedTenant);

                return Ok(tenant);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Tenant with ID {tenantId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while updating tenant: {ex.Message}");
            }
        }

        // DELETE: api/Tenant/5
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteTenant(int tenantId)
        {
            try
            {
                var isDeleted = await _tenantRepo.DeleteTenantAsync(tenantId);
                if (isDeleted)
                {
                    return NoContent();  // No content on successful delete
                }
                return NotFound($"Tenant with ID {tenantId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while deleting tenant: {ex.Message}");
            }
        }
    }
}
