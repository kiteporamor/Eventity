using Eventity.DataAccess.Converters;
using Eventity.DataAccess.Models;
using Eventity.Domain.Enums;
using Eventity.UnitTests.DalUnitTests.Fabrics;
using System;
using Allure.XUnit.Attributes.Steps;
using Xunit;

namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class ParticipationConverterTests
{
    [Fact]
    [AllureStep]
    public void ToDb_WhenValidParticipation_ReturnsParticipationDb()
    {
        var participation = ParticipationFactory.Organizer();

        var result = participation.ToDb();

        Assert.NotNull(result);
        Assert.Equal(participation.Id, result.Id);
        Assert.Equal(participation.UserId, result.UserId);
        Assert.Equal(participation.EventId, result.EventId);
        Assert.Equal(participation.Role, result.Role);
        Assert.Equal(participation.Status, result.Status);
    }

    [Fact]
    [AllureStep]
    public void ToDomain_WhenValidParticipationDb_ReturnsParticipation()
    {
        var participationDb = new ParticipationDb(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ParticipationRoleEnum.Participant,
            ParticipationStatusEnum.Rejected
        );

        var result = participationDb.ToDomain();

        Assert.NotNull(result);
        Assert.Equal(participationDb.Id, result.Id);
        Assert.Equal(participationDb.UserId, result.UserId);
        Assert.Equal(participationDb.EventId, result.EventId);
        Assert.Equal(participationDb.Role, result.Role);
        Assert.Equal(participationDb.Status, result.Status);
    }

    [Fact]
    [AllureStep]
    public void ToDb_WhenNullParticipation_ThrowsNullReferenceException()
    {
        Participation participation = null;

        Assert.Throws<NullReferenceException>(() => participation.ToDb());
    }

    [Fact]
    [AllureStep]
    public void ToDomain_WhenNullParticipationDb_ThrowsNullReferenceException()
    {
        ParticipationDb participationDb = null;

        Assert.Throws<NullReferenceException>(() => participationDb.ToDomain());
    }

    [Fact]
    [AllureStep]
    public void ToDb_WithDifferentRoleAndStatus_ConvertsCorrectly()
    {
        var participation = ParticipationFactory.RejectedParticipant();

        var result = participation.ToDb();

        Assert.Equal(ParticipationRoleEnum.Participant, result.Role);
        Assert.Equal(ParticipationStatusEnum.Rejected, result.Status);
    }

    [Fact]
    [AllureStep]
    public void ToDb_WithCustomParameters_ConvertsCorrectly()
    {
        var specificUserId = Guid.NewGuid();
        var specificEventId = Guid.NewGuid();

        var participation = ParticipationFactory.Create(
            userId: specificUserId,
            eventId: specificEventId,
            role: ParticipationRoleEnum.Participant,
            status: ParticipationStatusEnum.Accepted
        );

        var result = participation.ToDb();

        Assert.Equal(specificUserId, result.UserId);
        Assert.Equal(specificEventId, result.EventId);
        Assert.Equal(ParticipationRoleEnum.Participant, result.Role);
        Assert.Equal(ParticipationStatusEnum.Accepted, result.Status);
    }
}
