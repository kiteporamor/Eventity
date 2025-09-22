using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Attributes;

namespace Eventity.Domain.Models;

public class UserParticipationInfo
{
    public UserParticipationInfo() { }
    
    public UserParticipationInfo(Event eventItem, Guid organizerId, string organizerLogin)
    {
        EventItem = eventItem;
        OrganizerId = organizerId;
        OrganizerLogin = organizerLogin;
    }
    
    public Event EventItem { get; set; }

    [Required]
    public Guid OrganizerId { get; set; }
    
    [Required]
    public string OrganizerLogin { get; set; }
}
