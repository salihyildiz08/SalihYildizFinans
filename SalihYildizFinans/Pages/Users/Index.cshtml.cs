using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<AppUser> Users { get; set; } = new List<AppUser>();
        public Dictionary<string, string> UserRoles { get; set; } = new Dictionary<string, string>();

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.ToListAsync();

            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.Any() ? string.Join(", ", roles) : "User";
            }
        }
    }
}
