using Eventity.Domain.Models;
using Eventity.Web.Dtos;

namespace Eventity.Web.Converters;

public class ValidationDtoConverter
{
    public Validation ToDomain(ValidationDto validationDto)
    {
        return new Validation
        {
            CurrentUserId = validationDto.CurrentUserId,
            IsAdmin = validationDto.IsAdmin
        };
    }
}