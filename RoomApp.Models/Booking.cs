using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; } //booking name

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [ForeignKey("ApplicationUser")]
        public double UserId { get; set; } //user id who booked the room
        public ApplicationUser? User { get; set; }

    }
}