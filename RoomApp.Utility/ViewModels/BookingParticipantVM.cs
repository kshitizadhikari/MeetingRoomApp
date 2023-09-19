
using RoomApp.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace RoomApp.Utility.ViewModels
{
    public class BookingParticipantsVM
    {
        public int? BookingId { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "User Status is Required")]
        public ParticipantStatus? ParticipantStatus { get; set; } = Models.Enum.ParticipantStatus.Pending;
    }
}
