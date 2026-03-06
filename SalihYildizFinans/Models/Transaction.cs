using System.ComponentModel.DataAnnotations.Schema;

namespace SalihYildizFinans.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        
        public string Description { get; set; } // Nullable/Optional
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MyAmount { get; set; }

        public DateTime Date { get; set; }
        public string Period { get; set; } // yyyy-MM
        public TransactionType Type { get; set; } 

        // New: Status
        public bool IsPaid { get; set; } = false;

        public int? BankId { get; set; }
        public Bank? Bank { get; set; }

        // Data Isolation
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }

    public enum TransactionType
    {
        Borc,
        Alacak
    }
}
