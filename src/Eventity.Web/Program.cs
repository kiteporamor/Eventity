using System;
using System.Text;
using DataAccess;
using Eventity.Application.Services;
using Eventity.DataAccess.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Web;
<<<<<<< HEAD

var builder = WebApplication.CreateBuilder(args);

// Load additional configuration files based on environment or telemetry settings
var telemetryEnabled = builder.Configuration.GetValue<bool>("Telemetry:Enabled");
var additionalConfig = builder.Configuration.GetValue<string>("AdditionalConfig") ?? "default";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
=======
using MongoDB.Driver;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var loggingMode = builder.Configuration.GetValue<string>("Logging:Mode") ?? "Default";
var isExtendedLogging = loggingMode.Equals("Extended", StringComparison.OrdinalIgnoreCase);

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration);

if (isExtendedLogging)
{
    loggerConfig.MinimumLevel.Debug();
}

Log.Logger = loggerConfig.CreateLogger();
>>>>>>> b728086 (aaa)

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Host.UseSerilog();

<<<<<<< HEAD
// Add OpenTelemetry/Tracing
builder.Services.AddTelemetry(builder.Configuration);
=======
builder.Services.AddControllers();
builder.Services.AddOpenTelemetryIfEnabled(builder.Configuration);
>>>>>>> b728086 (aaa)

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization();

var isSwaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled") 
                       || builder.Environment.IsDevelopment();

builder.Services.AddEndpointsApiExplorer();
if (isSwaggerEnabled)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Eventity API", Version = "v1" });
        
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter JWT Bearer token **_only_**",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };
        
        c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() }
        });
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("UserOnly", policy => 
        policy.RequireRole("User"));
    
    options.AddPolicy("AdminOrUser", policy => 
        policy.RequireRole("Admin", "User"));
});

builder.Services.AddDataBase(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddDtoConverters();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventityDbContext>();
    // db.Database.Migrate();
}

if (isSwaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eventity API v1");
    });
}

app.UseRouting();
<<<<<<< HEAD
=======
app.UseMetricServer();
app.UseStaticFiles();
>>>>>>> b728086 (aaa)

app.UseCors("AllowAll");

if (isExtendedLogging)
{
    app.UseSerilogRequestLogging();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => "Healthy");

app.Run();

public partial class Program { }
<<<<<<< HEAD

=======
>>>>>>> b728086 (aaa)
