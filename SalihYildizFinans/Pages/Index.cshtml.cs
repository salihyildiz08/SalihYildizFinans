using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;
using System.Globalization;

namespace SalihYildizFinans.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // Updated Metrics (4 items)
        public decimal YearGeneralTotal { get; set; }  // 1. Genel Toplam (Paid+Unpaid)
        public decimal YearPaidTotal { get; set; }     // 2. Ödenen Genel Toplam
        public decimal YearMyShareTotal { get; set; }  // 3. Benim Payım Genel (Paid+Unpaid structure)
        public decimal YearReceivableTotal { get; set; } // 4. Alacak Genel (Paid+Unpaid structure)

        // Distribution Stats (Keep for charts if needed, or update)
        public decimal MonthTotalDebt { get; set; }
        public decimal MonthMyShare { get; set; }
        public decimal MonthOthersShare { get; set; }
        public string CurrentMonthName { get; set; }

        public decimal YearDistributionTotal { get; set; }
        public decimal YearDistributionMyShare { get; set; }
        public decimal YearDistributionOthersShare { get; set; }

        public IList<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        
        [BindProperty(SupportsGet = true)]
        public string SelectedPeriod { get; set; }


        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return;
            var userId = user.Id;

            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentPeriod = now.ToString("yyyy-MM");
            CurrentMonthName = now.ToString("MMMM yyyy", new CultureInfo("tr-TR")).ToUpper();

            // Fetch All Year Transactions
            var yearTransactions = await _context.Transactions
                .Where(t => t.AppUserId == userId && t.Date.Year == currentYear && t.Type == TransactionType.Borc)
                .ToListAsync();

            // 1. Yıl Genel Toplam (Paid + Unpaid)
            YearGeneralTotal = yearTransactions.Sum(t => t.TotalAmount);

            // 2. Yıl Toplam Ödenen
            YearPaidTotal = yearTransactions.Where(t => t.IsPaid).Sum(t => t.TotalAmount);

            // 3. Yıl Ödenmemiş Benim Payım
            // Filter by !IsPaid
            YearMyShareTotal = yearTransactions.Where(t => !t.IsPaid).Sum(t => t.MyAmount);

            // 4. Yıl Ödenmemiş Alacak (Others share of Unpaid)
            YearReceivableTotal = yearTransactions.Where(t => !t.IsPaid).Sum(t => t.TotalAmount - t.MyAmount);


            // Maintain Distribution Data Logic to match Detailed Cards (Unpaid Structure)
            // Let's align distribution chart with these new "Remaining" cards for consistency
            YearDistributionTotal = YearMyShareTotal + YearReceivableTotal; // Total Remaining
            YearDistributionMyShare = YearMyShareTotal;
            YearDistributionOthersShare = YearReceivableTotal;

            // Monthly
            var monthTransactions = yearTransactions
                .Where(t => t.Period == currentPeriod)
                .ToList();
            
            MonthTotalDebt = monthTransactions.Sum(t => t.TotalAmount);
            MonthMyShare = monthTransactions.Sum(t => t.MyAmount);
            MonthOthersShare = MonthTotalDebt - MonthMyShare;

            // Recent
             RecentTransactions = await _context.Transactions
                .Include(t => t.Bank)
                .Where(t => t.AppUserId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(5)
                .ToListAsync();
        }
    }
}
