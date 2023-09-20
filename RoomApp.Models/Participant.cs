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
        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public ParticipantStatus? Status { get; set; }
        public bool isDeleted { get; set; } = false;
    }
}
