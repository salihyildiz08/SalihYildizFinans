using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Hesaplar
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

        public IList<AccountPara> Accounts { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Accounts = await _context.AccountParas
                    .Include(a => a.Details)
                    .Where(a => a.AppUserId == user.Id)
                    .OrderBy(a => a.PersonName)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostCreateAccountAsync(AccountPara account)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            account.AppUserId = user.Id;
            _context.AccountParas.Add(account);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Hesap başarıyla oluşturuldu.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAccountAsync(int id, string personName, string currencyType)
        {
            var user = await _userManager.GetUserAsync(User);
            var account = await _context.AccountParas.FindAsync(id);

            if (user == null || account == null || account.AppUserId != user.Id) return Forbid();

            account.PersonName = personName;
            account.CurrencyType = currencyType;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Hesap güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var account = await _context.AccountParas.FindAsync(id);

            if (user == null || account == null || account.AppUserId != user.Id) return Forbid();

            _context.AccountParas.Remove(account);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Hesap silindi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddDetailAsync(int accountId, decimal amount, string description, DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            var account = await _context.AccountParas.FindAsync(accountId);

            if (user == null || account == null || account.AppUserId != user.Id) return Forbid();

            var detail = new AccountParaDetail
            {
                AccountParaId = accountId,
                Amount = amount,
                Description = description,
                Date = date == default ? DateTime.Now : date
            };

            _context.AccountParaDetails.Add(detail);
            
            // Ana toplamı güncelle
            account.TotalAmount += amount;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Hareket eklendi ve bakiye güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDetailAsync(int detailId)
        {
            var user = await _userManager.GetUserAsync(User);
            var detail = await _context.AccountParaDetails
                .Include(d => d.AccountPara)
                .FirstOrDefaultAsync(d => d.Id == detailId);

            if (user == null || detail == null || detail.AccountPara?.AppUserId != user.Id) return Forbid();

            // Ana toplamı geri al
            detail.AccountPara.TotalAmount -= detail.Amount;

            _context.AccountParaDetails.Remove(detail);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Hareket silindi ve bakiye güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateDetailAsync(int id, decimal amount, string description, DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            var detail = await _context.AccountParaDetails
                .Include(d => d.AccountPara)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (user == null || detail == null || detail.AccountPara?.AppUserId != user.Id) return Forbid();

            // Eski tutarı çıkar, yeni tutarı ekle (Bakiye farkı)
            detail.AccountPara.TotalAmount = (detail.AccountPara.TotalAmount - detail.Amount) + amount;

            detail.Amount = amount;
            detail.Description = description;
            detail.Date = date;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Hareket güncellendi ve bakiye senkronize edildi.";
            return RedirectToPage();
        }
    }
}
