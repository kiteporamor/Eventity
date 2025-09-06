using System;
using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Attributes;

namespace Eventity.Web.Dtos;

public class CreateEventRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [FutureDate]
    public DateTime DateTime { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; }
    
    [Required]
    public Guid OrganizerId { get; set; }
}