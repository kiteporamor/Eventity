using Eventity.DataAccess.Configurations;
using Eventity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventity.DataAccess.Context;

public class EventityDbContext : DbContext
{
    public virtual DbSet<UserDb> Users { get; set; }
    public virtual DbSet<EventDb> Events { get; set; }
    public virtual DbSet<NotificationDb> Notifications { get; set; }
    public virtual DbSet<ParticipationDb> Participations { get; set; }

    public EventityDbContext(DbContextOptions<EventityDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipationConfiguration());
    }
}
