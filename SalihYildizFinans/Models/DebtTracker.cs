using System;
using System.ComponentModel.DataAnnotations;

namespace SalihYildizFinans.Models
{
    public enum DebtTrackerType
    {
        Borc,
        Alacak
    }

    public class DebtTracker
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kişi adı gereklidir.")]
        public string PersonName { get; set; } = string.Empty;

        [Required]
        public DebtTrackerType Type { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Taksit sayısı en az 1 olmalıdır.")]
        public int TotalInstallments { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Kalan taksit sayısı 0'dan küçük olamaz.")]
        public int RemainingInstallments { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public decimal MonthlyAmount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Relation
        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}
