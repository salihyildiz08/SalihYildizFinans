using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Bankalar
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Bank> Banks { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            Banks = await _context.Banks
                .Where(b => b.AppUserId == user.Id || b.AppUserId == null) // Show personal and system banks
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(string Name)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            if (!string.IsNullOrEmpty(Name))
            {
                _context.Banks.Add(new Bank { Name = Name, AppUserId = user.Id });
                await _context.SaveChangesAsync();
                TempData["Message"] = "Banka başarıyla eklendi.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int Id, string Name)
        {
            var user = await _userManager.GetUserAsync(User);
            var bank = await _context.Banks.FindAsync(Id);
            
            if (user == null || bank == null || (bank.AppUserId != null && bank.AppUserId != user.Id)) 
                return Forbid();

            if (!string.IsNullOrEmpty(Name))
            {
                bank.Name = Name;
                _context.Banks.Update(bank);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Banka güncellendi.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            var bank = await _context.Banks.FindAsync(Id);

            if (user == null || bank == null || (bank.AppUserId != null && bank.AppUserId != user.Id)) 
                return Forbid();

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Banka silindi.";
            return RedirectToPage();
        }
    }
}
