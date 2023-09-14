﻿
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RoomApp.Models;
using RoomApp.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace RoomApp.Utility.ViewModels
{
    public class RoomBookingParticipantVM
    {

        public string? UserId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public int? BookingId { get; set; }
        public string? BookingName { get; set; }

        [Required]
        public DateTime? StartTime { get; set; }

        [Required]
        public DateTime? EndTime { get; set; }

        public List<Participant>? Participants { get; set; }

        public ParticipantStatus? UserStatus { get; set; }
    }
}
