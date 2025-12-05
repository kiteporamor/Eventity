// Eventity.Web.Controllers/AuthController.cs
using System;
using System.Threading.Tasks;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Eventity.Web.Converters;
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
        private readonly AuthDtoConverter _dtoConverter;

        public AuthController(IAuthService authService, AuthDtoConverter dtoConverter)
        {
            _authService = authService;
            _dtoConverter = dtoConverter;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.AuthenticateUser(request.Login, request.Password);
                
                if (result.Requires2FA)
                {
                    // Если требуется 2FA, возвращаем специальный ответ
                    return Ok(new 
                    {
                        Requires2FA = true,
                        UserId = result.TwoFactorUserId,
                        Message = "Verification code required. Please check your email."
                    });
                }
                
                return Ok(_dtoConverter.ToResponseDto(result));
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

        [HttpPost("verify-2fa")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequestDto request)
        {
            try
            {
                var result = await _authService.Verify2FA(request.UserId, request.Code);
                return Ok(_dtoConverter.ToResponseDto(result));
            }
            catch (Invalid2FACodeException ex)
            {
                return Unauthorized(new ErrorResponseDto { Message = ex.Message });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new ErrorResponseDto { Message = ex.Message });
            }
            catch (AuthServiceException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "Internal server error" });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterUser(
                    request.Name, request.Email, request.Login, request.Password, request.Role);
                return Ok(_dtoConverter.ToResponseDto(result));
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