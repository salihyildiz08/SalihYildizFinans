using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalihYildizFinans.Models
{
    public class VehicleTrade
    {
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime? SaleDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Expense { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetProfitLoss { get; set; }

        public string? Description { get; set; }
    }
}
