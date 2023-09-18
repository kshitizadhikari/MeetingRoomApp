﻿using RoomApp.DataAccess.DAL;
using RoomApp.DataAccess.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.Infrastructure.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppUserRepository _appUser;
        private readonly IRoomRepository _room;
        private readonly IBookingRepository _booking;
        private readonly IParticipantsRepository _participants;

        public RepositoryWrapper(AppDbContext appDbContext, IAppUserRepository userRepository, IRoomRepository roomRepository, IBookingRepository bookingRepository, IParticipantsRepository participantsRepository)
        {
            _appDbContext = appDbContext;
            _appUser = userRepository;
            _room = roomRepository;
            _booking = bookingRepository;
            _participants = participantsRepository;
        }

        public IRoomRepository Room => _room;

        public IBookingRepository Booking => _booking;

        public IParticipantsRepository Participants => _participants;

        public IAppUserRepository AppUser => _appUser;
        public async Task Save()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }

}
