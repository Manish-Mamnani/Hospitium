using Hospitium.Contracts;
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
            if (room.AvailableCount < room.TotalCount)
            {
                room.AvailableCount++;
            }

            await _context.SaveChangesAsync();
        }
    }
}