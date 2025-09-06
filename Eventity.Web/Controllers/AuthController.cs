using System;
using System.Threading.Tasks;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Eventity.Web.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)] // UserNotFound
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)] // InvalidPassword
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.AuthenticateUser(request.Login, request.Password);
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new ErrorResponseDto { Message = ex.Message });
            }
            catch (InvalidPasswordException ex)
            {
                return Unauthorized(new ErrorResponseDto { Message = ex.Message });
            }
            catch (AuthServiceException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "Internal server error" });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)] // UserAlreadyExists
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterUser(
                    request.Name, request.Email, request.Login, request.Password, request.Role);
                return Ok(result);
            }
            catch (UserAlreadyExistsException ex)
            {
                return Conflict(new ErrorResponseDto { Message = ex.Message });
            }
            catch (AuthServiceException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "Internal server error" });
            }
        }
    }
}