using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventity.Domain.Contracts;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventity.CoreService.Controllers;

[ApiController]
[Route("core/v1/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<ActionResult<User>> Add([FromBody] User user)
    {
        var created = await _userService.AddUser(
            user.Name, user.Email, user.Login, user.Password, user.Role);
        return Ok(created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<ActionResult<User>> GetById(Guid id)
    {
        var user = await _userService.GetUserById(id);
        return Ok(user);
    }

    [HttpGet("by-login/{login}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<ActionResult<User>> GetByLogin(string login)
    {
        var user = await _userService.GetUserByLogin(login);
        return Ok(user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetAll([FromQuery] string? login)
    {
        var users = await _userService.GetUsers(login);
        return Ok(users);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<ActionResult<User>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUser(id, request.Name, request.Email, request.Login, request.Password);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.RemoveUser(id);
        return NoContent();
    }
}
