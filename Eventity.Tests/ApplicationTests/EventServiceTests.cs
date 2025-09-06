using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventity.Application.Services;
using Eventity.Domain.Enums;
using Eventity.Domain.Exceptions;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eventity.Tests.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock = new();
        private readonly Mock<IParticipationRepository> _partRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly EventService _service;

        public EventServiceTests()
        {
            _service = new EventService(
                _eventRepoMock.Object,
                _partRepoMock.Object,
                new Logger<EventService>(new LoggerFactory()),
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task AddEvent_Should_Add_Event_And_Participation()
        {
            // Arrange
            var title = "Title";
            var desc = "Desc";
            var date = DateTime.Now;
            var addr = "Address";
            var organizerId = Guid.NewGuid();

            // Act
            var result = await _service.AddEvent(title, desc, date, addr, organizerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>()), Times.Once);
            _partRepoMock.Verify(x => x.AddAsync(It.IsAny<Participation>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetEventById_Should_Return_Event_If_Exists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var ev = new Event(id, "T", "D", DateTime.Now, "A", Guid.NewGuid());
            _eventRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(ev);

            // Act
            var result = await _service.GetEventById(id);

            // Assert
            Assert.Equal(ev, result);
        }

        [Fact]
        public async Task GetEventById_Should_Throw_If_Not_Exists()
        {
            // Arrange
            var id = Guid.NewGuid();
            _eventRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Event)null);

            // Act & Assert
            await Assert.ThrowsAsync<EventServiceException>(() => _service.GetEventById(id));
        }

        [Fact]
        public async Task GetAllEvents_Should_Return_List_If_Any()
        {
            // Arrange
            var list = new List<Event> { new(Guid.NewGuid(), "T", "D", DateTime.Now, "A", Guid.NewGuid()) };
            _eventRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetAllEvents();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllEvents_Should_Throw_If_Empty()
        {
            // Arrange
            _eventRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Event>());

            // Act & Assert
            await Assert.ThrowsAsync<EventServiceException>(() => _service.GetAllEvents());
        }

        [Fact]
        public async Task UpdateEvent_Should_Update_Existing_Event()
        {
            // Arrange
            var id = Guid.NewGuid();
            var originalEvent = new Event(id, "Old", "OldDesc", DateTime.Now, "OldAddr", Guid.NewGuid());
            _eventRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(originalEvent);
            _eventRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Event>())).ReturnsAsync((Event e) => e);

            // Act
            var result = await _service.UpdateEvent(id, "NewTitle", null, null, "NewAddress");

            // Assert
            Assert.Equal("NewTitle", result.Title);
            Assert.Equal("NewAddress", result.Address);
        }

        [Fact]
        public async Task UpdateEvent_Should_Throw_If_Not_Exists()
        {
            // Arrange
            _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Event)null);

            // Act & Assert
            await Assert.ThrowsAsync<EventServiceException>(() => _service.UpdateEvent(Guid.NewGuid(), "T", "D", DateTime.Now, "A"));
        }

        [Fact]
        public async Task RemoveEvent_Should_Call_Repository()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            await _service.RemoveEvent(id);

            // Assert
            _eventRepoMock.Verify(x => x.RemoveAsync(id), Times.Once);
        }

        [Fact]
        public async Task RemoveEvent_Should_Throw_On_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            _eventRepoMock.Setup(x => x.RemoveAsync(id)).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<EventServiceException>(() => _service.RemoveEvent(id));
        }
    }
}
