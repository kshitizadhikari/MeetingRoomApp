using RoomApp.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomApp.Models
{
    public class Participant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int? BookingId { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public ParticipantStatus? Status { get; set; }
    }
}
