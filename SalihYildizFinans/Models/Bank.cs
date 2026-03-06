using System.ComponentModel.DataAnnotations;

namespace SalihYildizFinans.Models
{
    public class Bank
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Data Isolation
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
