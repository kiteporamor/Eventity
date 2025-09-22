using Eventity.Domain.Models;

namespace Eventity.Web.Dtos;

public class UserParticipationInfoResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
    public string Address { get; set; }
    public Guid OrganizerId { get; set; }
    public string OrganizerLogin { get; set; }
}