using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Izinler
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

        public IList<Leave> Leaves { get; set; } = default!;
        
        // Özet Rapor Verileri (Sadece Tamamlanmamışlar için)
        public int SummaryTotalDays { get; set; }
        public int SummaryTotalHours { get; set; }
        public int SummaryTotalMinutes { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // VERİ İZOLASYONU: Sadece giriş yapan kullanıcının izinlerini getir.
            // Admin bile olsa başkasının iznini göremez.
            Leaves = await _context.Leaves
                .Where(l => l.AppUserId == user.Id)
                .OrderByDescending(l => l.StartDate) // En yeni en üstte
                .ToListAsync();

            // Özet Rapor Hesaplama (Sadece IsCompleted == false olanlar)
            var activeLeaves = Leaves.Where(l => !l.IsCompleted).ToList();
            
            int totalMins = activeLeaves.Sum(l => (l.TotalDays * 24 * 60) + (l.TotalHours * 60) + l.TotalMinutes);
            
            SummaryTotalDays = totalMins / (24 * 60);
            totalMins %= (24 * 60);
            SummaryTotalHours = totalMins / 60;
            SummaryTotalMinutes = totalMins % 60;
        }

        public async Task<IActionResult> OnPostCreateAsync(Leave leave)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            // Otomatik ata
            leave.AppUserId = user.Id;
            leave.IsCompleted = false; // Yeni kayıtta varsayılan olarak tamamlanmadı.

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "İzin kaydı başarıyla oluşturuldu.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(Leave leave)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var existing = await _context.Leaves.FindAsync(leave.Id);
            if (existing == null || existing.AppUserId != user.Id) return Forbid();

            // Güncelleme
            existing.Description = leave.Description;
            existing.StartDate = leave.StartDate;
            existing.EndDate = leave.EndDate;
            existing.TotalDays = leave.TotalDays;
            existing.TotalHours = leave.TotalHours;
            existing.TotalMinutes = leave.TotalMinutes;

            await _context.SaveChangesAsync();
            TempData["Message"] = "İzin kaydı güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null || leave.AppUserId != user.Id) return Forbid();

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();

            TempData["Message"] = "İzin kaydı silindi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null || leave.AppUserId != user.Id) return Forbid();

            leave.IsCompleted = !leave.IsCompleted;
            await _context.SaveChangesAsync();

            TempData["Message"] = leave.IsCompleted ? "İzin tamamlandı olarak işaretlendi." : "İzin beklemeye alındı.";
            return RedirectToPage();
        }
    }
}
