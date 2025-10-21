using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Attributes;

namespace Eventity.DataAccess.Models;

public class EventDb
{
    internal EventDb() { }
    
    public EventDb(Guid id, string title, string description, DateTime dateTime, string address, Guid organizerId)
    {
        Id = id;
        Title = title;
        Description = description;
        DateTime = dateTime;
        Address = address;
        OrganizerId = organizerId;
    }
    
    public Guid Id { get; set; }

    [Required] 
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    [FutureDate]
    public DateTime DateTime { get; set; }

    [Required]
    public string Address { get; set; }

    [Required] public Guid OrganizerId { get; set; }
    
    public virtual ICollection<ParticipationDb> Participations { get; set; } = new List<ParticipationDb>();
}
