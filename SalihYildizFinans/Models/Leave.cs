using System;
using System.ComponentModel.DataAnnotations;

namespace SalihYildizFinans.Models
{
    public class Leave
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Açıklama gereklidir.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }
        public int TotalHours { get; set; }
        public int TotalMinutes { get; set; }

        public bool IsCompleted { get; set; } = false;

        // Relation
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}
