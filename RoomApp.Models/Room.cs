using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Room name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Room size is required")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Room status is required")]
        public string? Status { get; set; } //occupied or free to use

        public ICollection<Booking>? Bookings { get; set; }
        public bool isDeleted { get; set; } = false;
    }
}
