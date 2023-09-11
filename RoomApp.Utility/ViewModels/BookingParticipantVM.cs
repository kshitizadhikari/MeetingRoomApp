
using RoomApp.Models.Enum;

namespace RoomApp.Utility.ViewModels
{
    public class BookingParticipantsVM
    {
        public int? BookingId { get; set; }

        public string? UserId { get; set; }

        public ParticipantStatus? ParticipantStatus { get; set; } = Models.Enum.ParticipantStatus.Pending;
    }
}
