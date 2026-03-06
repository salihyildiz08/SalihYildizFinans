using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalihYildizFinans.Models
{
    public class FieldAccount
    {
        public int Id { get; set; }

        [Required]
        public string PersonName { get; set; } = string.Empty;

        [Required]
        public string CurrencyType { get; set; } = "TL";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime Date { get; set; } = DateTime.Now;

        // Data Isolation
        [Required]
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}
