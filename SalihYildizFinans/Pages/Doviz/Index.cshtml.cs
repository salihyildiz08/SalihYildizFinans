using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Doviz
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

        public IList<CurrencyAsset> Assets { get; set; } = default!;

        // --- Altın ---
        public decimal GoldBuyQty { get; set; }
        public decimal GoldBuyTotal { get; set; }
        public decimal GoldSellQty { get; set; }
        public decimal GoldSellTotal { get; set; }
        public decimal GoldNetQty => GoldBuyQty - GoldSellQty;
        public decimal GoldProfitLoss { get; set; }

        // --- Gümüş ---
        public decimal SilverBuyQty { get; set; }
        public decimal SilverBuyTotal { get; set; }
        public decimal SilverSellQty { get; set; }
        public decimal SilverSellTotal { get; set; }
        public decimal SilverNetQty => SilverBuyQty - SilverSellQty;
        public decimal SilverProfitLoss { get; set; }

        // --- Dolar ---
        public decimal UsdBuyQty { get; set; }
        public decimal UsdBuyTotal { get; set; }
        public decimal UsdSellQty { get; set; }
        public decimal UsdSellTotal { get; set; }
        public decimal UsdNetQty => UsdBuyQty - UsdSellQty;
        public decimal UsdProfitLoss { get; set; }

        // --- Euro ---
        public decimal EurBuyQty { get; set; }
        public decimal EurBuyTotal { get; set; }
        public decimal EurSellQty { get; set; }
        public decimal EurSellTotal { get; set; }
        public decimal EurNetQty => EurBuyQty - EurSellQty;
        public decimal EurProfitLoss { get; set; }

        // --- Toplam ---
        public decimal TotalProfitLoss => GoldProfitLoss + SilverProfitLoss + UsdProfitLoss + EurProfitLoss;

        private void CalculateSummary(IEnumerable<CurrencyAsset> assets, CurrencyType type,
            out decimal buyQty, out decimal buyTotal, out decimal sellQty, out decimal sellTotal, out decimal profitLoss)
        {
            var items = assets.Where(a => a.Type == type).ToList();
            buyQty = items.Sum(a => a.Quantity);
            buyTotal = items.Sum(a => a.TotalPrice);
            sellQty = items.Sum(a => a.SellQuantity);
            sellTotal = items.Sum(a => a.SellTotalPrice);
            profitLoss = items.Where(a => a.IsSold).Sum(a => a.ProfitLoss);
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Assets = await _context.CurrencyAssets
                    .Where(a => a.AppUserId == user.Id)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                CalculateSummary(Assets, CurrencyType.Altin, out var gBQ, out var gBT, out var gSQ, out var gST, out var gPL);
                GoldBuyQty = gBQ; GoldBuyTotal = gBT; GoldSellQty = gSQ; GoldSellTotal = gST; GoldProfitLoss = gPL;

                CalculateSummary(Assets, CurrencyType.Gumus, out var sBQ, out var sBT, out var sSQ, out var sST, out var sPL);
                SilverBuyQty = sBQ; SilverBuyTotal = sBT; SilverSellQty = sSQ; SilverSellTotal = sST; SilverProfitLoss = sPL;

                CalculateSummary(Assets, CurrencyType.Dolar, out var uBQ, out var uBT, out var uSQ, out var uST, out var uPL);
                UsdBuyQty = uBQ; UsdBuyTotal = uBT; UsdSellQty = uSQ; UsdSellTotal = uST; UsdProfitLoss = uPL;

                CalculateSummary(Assets, CurrencyType.Euro, out var eBQ, out var eBT, out var eSQ, out var eST, out var ePL);
                EurBuyQty = eBQ; EurBuyTotal = eBT; EurSellQty = eSQ; EurSellTotal = eST; EurProfitLoss = ePL;
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(CurrencyAsset asset)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            asset.AppUserId = user.Id;
            asset.TotalPrice = asset.Quantity * asset.UnitPrice;
            _context.CurrencyAssets.Add(asset);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kayıt başarıyla eklendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, DateTime date, CurrencyType type, decimal quantity, decimal unitPrice)
        {
            var user = await _userManager.GetUserAsync(User);
            var asset = await _context.CurrencyAssets.FindAsync(id);

            if (user == null || asset == null || asset.AppUserId != user.Id) return Forbid();

            asset.Date = date;
            asset.Type = type;
            asset.Quantity = quantity;
            asset.UnitPrice = unitPrice;
            asset.TotalPrice = quantity * unitPrice;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Kayıt güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSellAsync(int id, DateTime sellDate, decimal sellQuantity, decimal sellUnitPrice)
        {
            var user = await _userManager.GetUserAsync(User);
            var asset = await _context.CurrencyAssets.FindAsync(id);

            if (user == null || asset == null || asset.AppUserId != user.Id) return Forbid();

            if (sellQuantity > asset.Quantity)
            {
                TempData["Error"] = "Satış miktarı alış miktarını aşamaz!";
                return RedirectToPage();
            }

            asset.SellDate = sellDate;
            asset.SellQuantity = sellQuantity;
            asset.SellUnitPrice = sellUnitPrice;
            asset.SellTotalPrice = sellQuantity * sellUnitPrice;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Satış kaydedildi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelSellAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var asset = await _context.CurrencyAssets.FindAsync(id);

            if (user == null || asset == null || asset.AppUserId != user.Id) return Forbid();

            asset.SellDate = null;
            asset.SellQuantity = 0;
            asset.SellUnitPrice = 0;
            asset.SellTotalPrice = 0;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Satış iptal edildi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAssetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var asset = await _context.CurrencyAssets.FindAsync(id);

            if (user == null || asset == null || asset.AppUserId != user.Id) return Forbid();

            _context.CurrencyAssets.Remove(asset);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kayıt silindi.";
            return RedirectToPage();
        }
    }
}
