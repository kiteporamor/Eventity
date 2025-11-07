using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventity.Domain.Enums;
using Eventity.Domain.Interfaces.Services;
using Eventity.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eventity.Application.Services;

public class JwtConfiguration
{
    public const string JwtSection = "Jwt";
    
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpireMinutes { get; set; } = 120;
}

public class JwtService : IJwtService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IOptions<JwtConfiguration> jwtOptions, ILogger<JwtService> logger)
    {
        _jwtConfig = jwtOptions.Value;
        _logger = logger;
    }

    public string GenerateToken(User user)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtConfig.ExpireMinutes),
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