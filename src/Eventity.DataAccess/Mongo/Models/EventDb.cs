using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Eventity.DataAccess.Models.Mongo;

public class EventDb
{
    [BsonId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
    public string Address { get; set; }
    public string OrganizerId { get; set; }
    public List<ParticipationDb> Participations { get; set; } = new();
}