using Hospitium.Contracts.Events;
using Hospitium.HotelService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly HotelDbContext _context;

        public BookingCreatedConsumer(HotelDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var roomId = context.Message.RoomId;

            var room = await _context.Rooms
                .AsTracking()
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                return;

            var requestedRooms = context.Message.NumberOfRooms > 0 ? context.Message.NumberOfRooms : 1;

            if (room.AvailableCount >= requestedRooms)
            {
                room.AvailableCount -= requestedRooms;
                await _context.SaveChangesAsync();
            }
        }
    }
}