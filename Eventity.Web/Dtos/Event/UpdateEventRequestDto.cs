using System;
using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Attributes;

namespace Eventity.Web.Dtos;

public class UpdateEventRequestDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [FutureDate]
    public DateTime? DateTime { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
}