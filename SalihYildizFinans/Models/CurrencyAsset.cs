using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalihYildizFinans.Models
{
    public class CurrencyAsset
    {
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }

        // --- Alış Bilgileri ---
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public CurrencyType Type { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // --- Satış Bilgileri ---
        public DateTime? SellDate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SellQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellUnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellTotalPrice { get; set; }

        // --- Hesaplamalar ---
        [NotMapped]
        public decimal RemainingQuantity => Quantity - SellQuantity;

        [NotMapped]
        public decimal ProfitLoss => SellTotalPrice - (SellQuantity * UnitPrice);

        [NotMapped]
        public bool IsSold => SellQuantity > 0;

        [NotMapped]
        public bool IsFullySold => SellQuantity >= Quantity;
    }

    public enum CurrencyType
    {
        Altin,
        Gumus,
        Dolar,
        Euro
    }
}
