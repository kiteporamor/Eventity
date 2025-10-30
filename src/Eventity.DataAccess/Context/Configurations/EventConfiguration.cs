using System;
using Eventity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventity.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<EventDb>
{
    public void Configure(EntityTypeBuilder<EventDb> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).
            ValueGeneratedNever();

        builder.HasMany(e => e.Participations)
            .WithOne(p => p.Event)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
