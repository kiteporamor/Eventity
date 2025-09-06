using Eventity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventity.DataAccess.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationDb>
{
    public void Configure(EntityTypeBuilder<NotificationDb> builder)
    {
        builder.HasKey(e => e.Id);
    
        builder.Property(e => e.Id).
            ValueGeneratedNever();

        builder.HasOne(n => n.Participation)
            .WithMany(p => p.Notifications)
            .HasForeignKey(n => n.ParticipationId)
            .IsRequired();
    }
}
