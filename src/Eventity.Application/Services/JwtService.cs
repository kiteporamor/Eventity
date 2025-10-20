using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Eventity.Application.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credentials);

            _logger.LogInformation("JWT token generated for user {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user {UserId}", user.Id);
            throw;
        }
    }
}
