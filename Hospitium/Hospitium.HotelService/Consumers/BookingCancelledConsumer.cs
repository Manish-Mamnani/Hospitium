using Hospitium.Contracts.Events;
using Hospitium.HotelService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly HotelDbContext _context;

        public BookingCancelledConsumer(HotelDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            var roomId = context.Message.RoomId;

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                return;

            // 🔄 Restore availability
            var requestedRooms = context.Message.NumberOfRooms > 0 ? context.Message.NumberOfRooms : 1;

            if (room.AvailableCount + requestedRooms <= room.TotalCount)
            {
                room.AvailableCount += requestedRooms;
            }
            else
            {
                room.AvailableCount = room.TotalCount;
            }

            await _context.SaveChangesAsync();
        }
    }
}