﻿using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.Model;
using workstream.DTO;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using workstream.Services;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;

        public UserController(UserRepo userRepo, IMapper mapper, JwtService jwtService)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        // Create a new User
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserWriteDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("User data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(userDTO.Username))
            {
                return BadRequest("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(userDTO.PasswordHash))
            {
                return BadRequest("Password cannot be empty.");
            }

            try
            {
                // Get the token from the Authorization header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Extract TenantId from the token
                var tenantId = _jwtService.GetTenantIdFromToken(token);
                if (tenantId == 0)  
                {
                    return Unauthorized("Invalid or missing tenant information.");
                }

                // Map DTO to Model and assign TenantId
                var user = _mapper.Map<User>(userDTO);
                user.TenantId = tenantId; // Ensure the user is linked to the correct tenant

                await _userRepo.CreateUserAsync(user);

                // Map back to DTO for returning
                var userReadDTO = _mapper.Map<UserReadDTO>(user);

                //return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = user.UserId }, userReadDTO);
                return Ok(new { Message = "User created successfully.", User = userReadDTO });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            try
            {
                // Get the token from the Authorization header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Get TenantId from the token using JwtService
                var tenantId = _jwtService.GetTenantIdFromToken(token);

                // Get users filtered by TenantId
                var users = await _userRepo.GetAllUsersAsync(tenantId);

                // Map users to DTOs if necessary
                var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);

                return Ok(userReadDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get a User by ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                // Map model to DTO for returning
                var userReadDTO = _mapper.Map<UserReadDTO>(user);

                return Ok(userReadDTO);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Update an existing User
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UserUpdateDTO updatedUserDTO)
        {
            if (updatedUserDTO == null)
            {
                return BadRequest("Updated user data cannot be null.");
            }

            try
            {
                // Map DTO to Model
                var updatedUser = _mapper.Map<User>(updatedUserDTO);

                var result = await _userRepo.UpdateUserAsync(userId, updatedUser);

                // Map back to DTO for returning
                var userReadDTO = _mapper.Map<UserReadDTO>(result);

                return Ok(userReadDTO);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Delete a User
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            try
            {
                var result = await _userRepo.DeleteUserAsync(userId);

                if (!result)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
