using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Tarlalar
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

        public IList<FieldAccount> FieldAccounts { get; set; } = default!;
        public List<SummaryReport> PersonSummaries { get; set; } = new();
        public List<SummaryReport> CurrencySummaries { get; set; } = new();

        public class SummaryReport
        {
            public string Label { get; set; } = string.Empty;
            public string Currency { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal RemainingAmount => TotalAmount - PaidAmount;
        }

        // Pagination Properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync(int p = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                CurrentPage = p < 1 ? 1 : p;

                // 1. Base Query
                var query = _context.FieldAccounts
                    .Where(f => f.AppUserId == user.Id);

                // 2. Calculate Total Count & Pages
                TotalCount = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

                // 3. Fetch Paginated Data
                FieldAccounts = await query
                    .OrderByDescending(f => f.Date)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // 4. Summaries (Database Side) - Optimized
                // Note: EF Core 8+ supports GroupBy better, but for complex projections sometimes client eval is needed. 
                // However, we should try to keep it server side if possible or minimize data fetched.
                // Fetching all for summary is still heavy if millions of rows, but better than fetching all entities.
                // We will fetch minimal data needed for summary.
                
                var allDataForSummary = await _context.FieldAccounts
                    .Where(f => f.AppUserId == user.Id)
                    .Select(f => new { f.PersonName, f.CurrencyType, f.Amount, f.IsPaid })
                    .ToListAsync(); // Still fetching all light objects, but much lighter than full entities. 
                                      // Ideally we do GroupBy in SQL, but let's stick to this for compatibility unless EF Core version guarantees GroupBy translation.

                // Person Based Summary
                PersonSummaries = allDataForSummary
                    .GroupBy(f => new { f.PersonName, f.CurrencyType })
                    .Select(g => new SummaryReport
                    {
                        Label = g.Key.PersonName,
                        Currency = g.Key.CurrencyType,
                        TotalAmount = g.Sum(x => x.Amount),
                        PaidAmount = g.Where(x => x.IsPaid).Sum(x => x.Amount)
                    })
                    .OrderBy(s => s.Label)
                    .ToList();

                // Currency Based Summary
                CurrencySummaries = allDataForSummary
                    .GroupBy(f => f.CurrencyType)
                    .Select(g => new SummaryReport
                    {
                        Label = g.Key,
                        Currency = g.Key,
                        TotalAmount = g.Sum(x => x.Amount),
                        PaidAmount = g.Where(x => x.IsPaid).Sum(x => x.Amount)
                    })
                    .OrderBy(s => s.Label)
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(FieldAccount fieldAccount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            fieldAccount.AppUserId = user.Id;
            _context.FieldAccounts.Add(fieldAccount);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kayıt başarıyla eklendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(FieldAccount fieldAccount)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.FieldAccounts.FindAsync(fieldAccount.Id);

            if (user == null || existing == null || existing.AppUserId != user.Id) return Forbid();

            existing.PersonName = fieldAccount.PersonName;
            existing.CurrencyType = fieldAccount.CurrencyType;
            existing.Amount = fieldAccount.Amount;
            existing.Description = fieldAccount.Description;
            existing.IsPaid = fieldAccount.IsPaid;
            existing.Date = fieldAccount.Date;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Kayıt güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.FieldAccounts.FindAsync(id);

            if (user == null || existing == null || existing.AppUserId != user.Id) return Forbid();

            _context.FieldAccounts.Remove(existing);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kayıt silindi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTogglePaidAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.FieldAccounts.FindAsync(id);

            if (user == null || existing == null || existing.AppUserId != user.Id) return Forbid();

            existing.IsPaid = !existing.IsPaid;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
