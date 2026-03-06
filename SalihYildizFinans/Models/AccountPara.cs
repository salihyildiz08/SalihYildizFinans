using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalihYildizFinans.Models
{
    public class AccountPara
    {
        public int Id { get; set; }

        [Required]
        public string PersonName { get; set; } = string.Empty;

        [Required]
        public string CurrencyType { get; set; } = "TL"; // TL, USD, EUR vb.

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Veri İzolasyonu
        [Required]
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }

        public ICollection<AccountParaDetail> Details { get; set; } = new List<AccountParaDetail>();
    }

    public class AccountParaDetail
    {
        public int Id { get; set; }

        public int AccountParaId { get; set; }
        public AccountPara? AccountPara { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
