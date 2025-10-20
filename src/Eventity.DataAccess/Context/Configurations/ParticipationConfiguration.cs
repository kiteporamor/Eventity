using Eventity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventity.DataAccess.Configurations;

public class ParticipationConfiguration : IEntityTypeConfiguration<ParticipationDb>
{
    public void Configure(EntityTypeBuilder<ParticipationDb> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.HasOne(p => p.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Event)
            .WithMany(e => e.Participations)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.Notifications)
            .WithOne(n => n.Participation)
            .HasForeignKey(n => n.ParticipationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}