using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Taksitler
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<DebtTracker> Items { get; set; } = default!;
        public List<PersonSummary> Summaries { get; set; } = new();

        public class PersonSummary
        {
            public string PersonName { get; set; } = "";
            public decimal TotalBorc { get; set; }
            public decimal TotalAlacak { get; set; }
            public decimal ThisMonthTotal { get; set; }
            public decimal RemainingBorc { get; set; }
            public decimal RemainingAlacak { get; set; }
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // VERİ İZOLASYONU: Sadece giriş yapan kullanıcının verileri.
            // Kalan Taksit > 0 olanlar gösterilecek.
            Items = await _context.DebtTrackers
                .Where(x => x.AppUserId == user.Id && x.RemainingInstallments > 0)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            // Kişi Bazlı Özet Raporlama
            Summaries = Items.GroupBy(x => x.PersonName)
                .Select(g => new PersonSummary
                {
                    PersonName = g.Key,
                    TotalBorc = g.Where(x => x.Type == DebtTrackerType.Borc).Sum(x => x.TotalAmount),
                    TotalAlacak = g.Where(x => x.Type == DebtTrackerType.Alacak).Sum(x => x.TotalAmount),
                    ThisMonthTotal = g.Sum(x => x.MonthlyAmount),
                    RemainingBorc = g.Where(x => x.Type == DebtTrackerType.Borc).Sum(x => x.MonthlyAmount * x.RemainingInstallments),
                    RemainingAlacak = g.Where(x => x.Type == DebtTrackerType.Alacak).Sum(x => x.MonthlyAmount * x.RemainingInstallments)
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync(DebtTracker item)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            item.AppUserId = user.Id;
            _context.DebtTrackers.Add(item);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "Kayıt başarıyla eklendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(DebtTracker item)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var existing = await _context.DebtTrackers.FindAsync(item.Id);
            if (existing == null || existing.AppUserId != user.Id) return Forbid();

            existing.PersonName = item.PersonName;
            existing.Type = item.Type;
            existing.TotalInstallments = item.TotalInstallments;
            existing.RemainingInstallments = item.RemainingInstallments;
            existing.TotalAmount = item.TotalAmount;
            existing.MonthlyAmount = item.MonthlyAmount;
            existing.Description = item.Description;
            existing.Date = item.Date;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Kayıt güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReduceInstallmentAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var item = await _context.DebtTrackers.FindAsync(id);
            if (item == null || item.AppUserId != user.Id) return Forbid();

            if (item.RemainingInstallments > 0)
            {
                item.RemainingInstallments--;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Taksit başarıyla düşüldü. Kalan: {item.RemainingInstallments}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var item = await _context.DebtTrackers.FindAsync(id);
            if (item == null || item.AppUserId != user.Id) return Forbid();

            _context.DebtTrackers.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Kayıt silindi.";
            return RedirectToPage();
        }
    }
}
