using System;
using System.Collections.Generic;
using System.Linq;
using Eventity.Application.Services;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Services;
using Eventity.Web.Converters;
using Eventity.Web.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Eventity.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly UserDtoConverter _dtoConverter;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        UserDtoConverter dtoConverter)
    {
        _userService = userService;
        _logger = logger;
        _dtoConverter = dtoConverter;
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserById(userId);
            return Ok(_dtoConverter.ToResponseDto(user));
        }
        catch (UserServiceException ex)
        {
            _logger.LogWarning(ex, "Failed to get current user");
            return Unauthorized(new ProblemDetails
            {
                Title = "Access denied",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
    {
        try
        {
            var user = await _userService.GetUserById(id);
            return Ok(_dtoConverter.ToResponseDto(user));
        }
        catch (UserServiceException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", id);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserResponseDto>>> GetUsers(string? login)
    {
        try
        {
            var users = await _userService.GetUsers(login);
            return Ok(users.Select(_dtoConverter.ToResponseDto));
        }
        catch (UserServiceException ex)
        {
            _logger.LogWarning(ex, "Failed to get users");
            return Unauthorized(new ProblemDetails
            {
                Title = "Access denied",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequestDto requestDto)
    {
        try
        {
            if (!CanModifyUser(id))
                return Forbid();

            var user = await _userService.UpdateUser(
                id,
                requestDto.Name,
                requestDto.Email,
                requestDto.Login,
                requestDto.Password);

            return Ok(_dtoConverter.ToResponseDto(user));
        }
        catch (UserServiceException ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Update failed",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.RemoveUser(id);
            return NoContent();
        }
        catch (UserServiceException ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", id);
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId);
    }

    private bool CanModifyUser(Guid targetUserId)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        return isAdmin || currentUserId == targetUserId;
    }
}
