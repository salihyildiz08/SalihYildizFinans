using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;
using System.Globalization;

namespace SalihYildizFinans.Pages
{
    [Authorize]
    public class BorclarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BorclarModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Transaction> Transactions { get; set; } = default!;
        public SelectList BankList { get; set; }
        
        public Dictionary<string, string> PeriodList { get; set; } = new Dictionary<string, string>();

        [BindProperty(SupportsGet = true, Name = "period")]
        public string SelectedPeriod { get; set; }

        public decimal PeriodTotalDebt { get; set; }
        public decimal PeriodMyShare { get; set; }
        public decimal PeriodOthersShare { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return;
            var userId = user.Id;

            var culture = new CultureInfo("tr-TR");
            var now = DateTime.Now;
            
            for (int i = -12; i <= 24; i++)
            {
                var dt = now.AddMonths(i);
                var key = dt.ToString("yyyy-MM");
                var value = dt.ToString("MMMM yyyy", culture).ToUpper(); 
                if (!PeriodList.ContainsKey(key))
                {
                    PeriodList.Add(key, value);
                }
            }
            PeriodList = PeriodList.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            if (string.IsNullOrEmpty(SelectedPeriod))
            {
                SelectedPeriod = now.ToString("yyyy-MM");
            }

            BankList = new SelectList(await _context.Banks.ToListAsync(), "Id", "Name");

            // FILTER BY USER ID
            var query = _context.Transactions
                .Include(t => t.Bank)
                .Where(t => t.AppUserId == userId && t.Period == SelectedPeriod);

            Transactions = await query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToListAsync();

            var allDebts = Transactions.Where(t => t.Type == TransactionType.Borc).ToList();

            PeriodTotalDebt = allDebts.Sum(t => t.TotalAmount);
            PeriodMyShare = allDebts.Sum(t => t.MyAmount);
            PeriodOthersShare = PeriodTotalDebt - PeriodMyShare;
        }

        public async Task<IActionResult> OnGetExport(string period)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            // Filter by Period and User
            var transactions = await _context.Transactions
                .Include(t => t.Bank)
                .Where(t => t.AppUserId == user.Id && t.Period == period)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Tarih;Aciklama;Banka;Toplam Tutar;Benim Payim;Durum");

            foreach (var t in transactions)
            {
                var status = t.IsPaid ? "Odendi" : "Odenmedi";
                var bankName = t.Bank?.Name ?? "Diger";
                // Replace ; with , to avoid CSV breakage
                var desc = t.Description?.Replace(";", ",") ?? ""; 
                
                builder.AppendLine($"{t.Date.ToShortDateString()};{desc};{bankName};{t.TotalAmount};{t.MyAmount};{status}");
            }

            var fileName = $"Ekstre_{period}.csv";
            // UTF-8 BOM for Excel
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            
            return File(bytes, "text/csv", fileName);
        }

        public async Task<IActionResult> OnPostCreateOrUpdateAsync(Transaction transaction)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return RedirectToPage("/Login");
            var userId = user.Id;

            transaction.Type = TransactionType.Borc;

            DateTime calculatedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(transaction.Period))
            {
                var parts = transaction.Period.Split('-');
                if (parts.Length == 2)
                {
                    int year = int.Parse(parts[0]);
                    int month = int.Parse(parts[1]);
                    if (year == DateTime.Now.Year && month == DateTime.Now.Month)
                    {
                        calculatedDate = DateTime.Now;
                    } 
                    else 
                    {
                        calculatedDate = new DateTime(year, month, 1);
                    }
                }
            }

            if (transaction.Id == 0)
            {
                // New Transaction - Assign User
                transaction.AppUserId = userId; 
                transaction.Date = calculatedDate;
                _context.Transactions.Add(transaction);
                TempData["Message"] = "Kayıt başarıyla eklendi.";
            }
            else
            {
                // Update - Ensure User Owns it
                var existing = await _context.Transactions.FindAsync(transaction.Id);
                if (existing != null)
                {
                   if (existing.AppUserId != userId)
                   {
                        return Forbid(); // Should not happen in UI but security measure
                   }

                    if (existing.IsPaid)
                    {
                        TempData["Error"] = "Ödenmiş kayıtlar düzenlenemez. Önce ödeme durumunu kaldırın.";
                        return RedirectToPage(new { period = transaction.Period });
                    }

                    if (existing.Period != transaction.Period)
                    {
                        existing.Date = calculatedDate; 
                    }

                    existing.Description = transaction.Description;
                    existing.BankId = transaction.BankId;
                    existing.Period = transaction.Period;
                    existing.TotalAmount = transaction.TotalAmount;
                    existing.MyAmount = transaction.MyAmount;
                    
                    _context.Transactions.Update(existing);
                    TempData["Message"] = "Kayıt güncellendi.";
                }
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage(new { period = transaction.Period });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string period)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return RedirectToPage("/Login");

            var t = await _context.Transactions.FindAsync(id);
            if (t != null)
            {
                if(t.AppUserId != user.Id) return Forbid();

                if (t.IsPaid)
                {
                     TempData["Error"] = "Ödenmiş kayıtlar silinemez. Önce ödeme durumunu kaldırın.";
                     return RedirectToPage(new { period = period });
                }

                _context.Transactions.Remove(t);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kayıt silindi.";
            }
            return RedirectToPage(new { period = period });
        }
        
        public async Task<IActionResult> OnPostTogglePaidAsync(int id, string period)
        {
             var user = await _userManager.GetUserAsync(User);
             if(user == null) return RedirectToPage("/Login");

             var t = await _context.Transactions.FindAsync(id);
            if (t != null)
            {
                if(t.AppUserId != user.Id) return Forbid();

                t.IsPaid = !t.IsPaid;
                await _context.SaveChangesAsync();
                TempData["Message"] = t.IsPaid ? "Ödendi olarak işaretlendi." : "Ödenmedi olarak işaretlendi.";
            }
            return RedirectToPage(new { period = period });
        }
    }
}
