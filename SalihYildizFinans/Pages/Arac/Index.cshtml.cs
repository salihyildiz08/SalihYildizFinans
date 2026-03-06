using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Arac
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

        public IList<VehicleTrade> Trades { get; set; } = default!;
        
        public int TotalVehicleCount { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalLoss { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Trades = await _context.VehicleTrades
                    .Where(t => t.AppUserId == user.Id)
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToListAsync();

                TotalVehicleCount = Trades.Count;
                TotalProfit = Trades.Where(t => t.NetProfitLoss > 0).Sum(t => t.NetProfitLoss);
                TotalLoss = Trades.Where(t => t.NetProfitLoss < 0).Sum(t => Math.Abs(t.NetProfitLoss));
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(VehicleTrade trade)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            trade.AppUserId = user.Id;
            // NetProfitLoss is calculated on client-side and sent, but let's ensure it here too
            trade.NetProfitLoss = trade.SalePrice - (trade.PurchasePrice + trade.Expense);
            
            _context.VehicleTrades.Add(trade);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Araç kaydı başarıyla eklendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, DateTime purchaseDate, DateTime? saleDate, decimal purchasePrice, decimal salePrice, decimal expense, string? description)
        {
            var user = await _userManager.GetUserAsync(User);
            var trade = await _context.VehicleTrades.FindAsync(id);

            if (user == null || trade == null || trade.AppUserId != user.Id) return Forbid();

            trade.PurchaseDate = purchaseDate;
            trade.SaleDate = saleDate;
            trade.PurchasePrice = purchasePrice;
            trade.SalePrice = salePrice;
            trade.Expense = expense;
            trade.NetProfitLoss = salePrice - (purchasePrice + expense);
            trade.Description = description;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Araç kaydı güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteTradeAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var trade = await _context.VehicleTrades.FindAsync(id);

            if (user == null || trade == null || trade.AppUserId != user.Id) return Forbid();

            _context.VehicleTrades.Remove(trade);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Araç kaydı silindi.";
            return RedirectToPage();
        }
    }
}
