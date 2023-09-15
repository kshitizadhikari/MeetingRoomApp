using RoomApp.DataAccess.DAL;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using RoomApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.Infrastructure.Repositories
{
    public class BookingRepository: RepositoryBase<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext appDbContext)
            : base(appDbContext)
        {
        }
    }
}
