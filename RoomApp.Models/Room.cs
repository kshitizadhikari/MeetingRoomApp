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

        [Required]
        [Range(1, 20)]
        public int Size { get; set; }

        [Required]
        public string? Status { get; set; } //occupied or free to use
    }
}
