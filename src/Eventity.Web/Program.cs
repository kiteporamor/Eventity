using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Web;
using Eventity.Domain.Interfaces.Services;
using Eventity.Web.CoreClients;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

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

void ConfigureCoreClient(HttpClient client)
{
    var baseUrl = builder.Configuration["ServiceUrls:CoreService"]
                  ?? "http://core-service:5002";
    client.BaseAddress = new Uri(baseUrl);
}

builder.Services.AddHttpClient<IAuthService, CoreAuthServiceClient>(ConfigureCoreClient);
builder.Services.AddHttpClient<IEventService, CoreEventServiceClient>(ConfigureCoreClient);
builder.Services.AddHttpClient<INotificationService, CoreNotificationServiceClient>(ConfigureCoreClient);
builder.Services.AddHttpClient<IParticipationService, CoreParticipationServiceClient>(ConfigureCoreClient);
builder.Services.AddHttpClient<IUserService, CoreUserServiceClient>(ConfigureCoreClient);
builder.Services.AddDtoConverters();

var app = builder.Build();

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

var isReadOnly = builder.Configuration.GetValue<bool>("ReadOnly");
if (isReadOnly)
{
    app.Use(async (context, next) =>
    {
        var method = context.Request.Method;
        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Read-only instance does not allow write operations.");
            return;
        }

        await next();
    });
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

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/v1/health", () => "Healthy");

app.Run();

public partial class Program { }
