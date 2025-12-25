using System;
using Eventity.Domain.Enums;
using Eventity.Domain.Models;

namespace Eventity.Domain.Contracts;

public class AuthLoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRoleEnum Role { get; set; }
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
}

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DateTime { get; set; }
    public string? Address { get; set; }
    public Validation Validation { get; set; } = new();
}

public class ValidationRequest
{
    public Validation Validation { get; set; } = new();
}

public class AddParticipationRequest
{
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public ParticipationRoleEnum Role { get; set; }
    public ParticipationStatusEnum Status { get; set; }
    public Validation Validation { get; set; } = new();
}

public class UpdateParticipationRequest
{
    public ParticipationStatusEnum? Status { get; set; }
    public Validation Validation { get; set; } = new();
}

public class ChangeParticipationStatusRequest
{
    public ParticipationStatusEnum Status { get; set; }
}

public class ChangeParticipationRoleRequest
{
    public ParticipationRoleEnum Role { get; set; }
}

public class NotificationCreateRequest
{
    public Guid EventId { get; set; }
    public NotificationTypeEnum Type { get; set; }
    public Validation Validation { get; set; } = new();
}

public class NotificationUpdateRequest
{
    public Guid? ParticipationId { get; set; }
    public string? Text { get; set; }
    public DateTime? SentAt { get; set; }
}

public class NotificationFilterRequest
{
    public Guid? ParticipationId { get; set; }
    public Validation Validation { get; set; } = new();
}

public class ParticipationUserInfoRequest
{
    public string? OrganizerLogin { get; set; }
    public string? EventTitle { get; set; }
    public Guid? UserId { get; set; }
    public Validation Validation { get; set; } = new();
}
