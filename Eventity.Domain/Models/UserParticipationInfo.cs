using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Attributes;

namespace Eventity.Domain.Models;

public class UserParticipationInfo
{
    public UserParticipationInfo() { }
    
    public UserParticipationInfo(Event eventItem, string organizerLogin)
    {
        EventItem = eventItem;
        OrganizerLogin = organizerLogin;
    }
    
    public Event EventItem { get; set; }
    
    [Required]
    public string OrganizerLogin { get; set; }
}
