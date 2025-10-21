using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class EventDtoConverter
{
    public EventResponseDto ToResponseDto(Event eventItem)
    {
        return new EventResponseDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            DateTime = eventItem.DateTime,
            Address = eventItem.Address,
            OrganizerId = eventItem.OrganizerId
        };
    }
}