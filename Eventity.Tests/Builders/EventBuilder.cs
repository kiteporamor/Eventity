namespace Eventity.UnitTests.DalUnitTests.ConvertersUnitTests;

public class EventBuilder
{
    private Event _event;

    public EventBuilder()
    {
        _event = new Event(
            Guid.NewGuid(),
            "Default event",
            "Default description",
            DateTime.Now.AddDays(1),
            "Default address",
            Guid.NewGuid()
        );
    }

    public EventBuilder With(Action<Event> configure)
    {
        configure(_event);
        return this;
    }

    public EventBuilder WithId(Guid id) => With(e => e.Id = id);
    public EventBuilder WithTitle(string title) => With(e => e.Title = title);
    public EventBuilder WithDescription(string description) => With(e => e.Description = description);
    public EventBuilder WithDateTime(DateTime dateTime) => With(e => e.DateTime = dateTime);
    public EventBuilder WithAddress(string address) => With(e => e.Address = address);
    
    public Event Build() => _event;
    public static Event Default() => new EventBuilder().Build();
}