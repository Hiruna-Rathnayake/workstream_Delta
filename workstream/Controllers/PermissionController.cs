using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.Model;
using AutoMapper;
using Microsoft.Extensions.Logging;
using workstream.DTO;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionRepo _permissionRepo;
        private readonly ILogger<PermissionController> _logger;
        private readonly IMapper _mapper;

        public PermissionController(PermissionRepo permissionRepo, ILogger<PermissionController> logger, IMapper mapper)
        {
            _permissionRepo = permissionRepo;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: api/permissions
        [HttpGet]
        public async Task<ActionResult<List<PermissionReadDTO>>> GetPermissionsAsync()
        {
            try
            {
                var permissions = await _permissionRepo.GetAllPermissionsAsync();
                var permissionDtos = _mapper.Map<List<PermissionReadDTO>>(permissions);
                return Ok(permissionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching permissions: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/permissions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionReadDTO>> GetPermissionByIdAsync(int id)
        {
            try
            {
                var permission = await _permissionRepo.GetPermissionByIdAsync(id);
                var permissionDto = _mapper.Map<PermissionReadDTO>(permission);
                return Ok(permissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching permission with ID {PermissionId}: {Message}", id, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

