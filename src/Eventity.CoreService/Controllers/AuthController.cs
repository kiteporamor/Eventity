using System;
using System.Threading.Tasks;
using Eventity.Domain.Contracts;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.CoreService.Controllers;

[ApiController]
[Route("core/v1/auth")]
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
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        var result = await _authService.AuthenticateUser(request.Login, request.Password);
        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] AuthRegisterRequest request)
    {
        var result = await _authService.RegisterUser(
            request.Name, request.Email, request.Login, request.Password, request.Role);
        return Ok(result);
    }
}
