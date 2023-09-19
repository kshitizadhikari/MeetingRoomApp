using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.Infrastructure.Interfaces
{
    public interface IRepositoryWrapper
    {
        IRoomRepository Room { get; }
        IBookingRepository Booking { get; }
        IParticipantsRepository Participants { get; }
        IAppUserRepository AppUsers { get;  }
        Task Save();
    }
}
