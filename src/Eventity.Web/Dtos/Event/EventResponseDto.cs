using System;

namespace Eventity.Web.Dtos;

public class EventResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
    public string Address { get; set; }
    public Guid OrganizerId { get; set; }
}