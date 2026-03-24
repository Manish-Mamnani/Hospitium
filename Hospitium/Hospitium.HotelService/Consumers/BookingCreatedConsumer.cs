using Hospitium.Contracts;
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

            if (room.AvailableCount > 0)
            {
                room.AvailableCount--;
                await _context.SaveChangesAsync();
            }
        }
    }
}