using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public CreateModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "E-Posta")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre (Geçici)")]
            public string Password { get; set; }

            [Required]
            [Display(Name = "Rol")]
            public string Role { get; set; } // Admin, User
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new AppUser
            {
                UserName = Input.Email, // Default Username = Email
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                EmailConfirmed = true 
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Input.Role);
                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
