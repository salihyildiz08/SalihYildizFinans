using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public AppUser AppUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            AppUser = await _userManager.FindByIdAsync(id);

            if (AppUser == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                // Prevent deleting self
                if (user.UserName == User.Identity.Name)
                {
                    return RedirectToPage("./Index"); // Show error ideal but simple redirect for now
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage("./Index");
        }
    }
}
