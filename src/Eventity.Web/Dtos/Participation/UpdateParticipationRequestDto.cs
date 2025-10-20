using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Enums;

namespace Eventity.Web.Dtos;

public class UpdateParticipationRequestDto
{
    [EnumDataType(typeof(ParticipationStatusEnum))]
    public ParticipationStatusEnum? Status { get; set; }
}