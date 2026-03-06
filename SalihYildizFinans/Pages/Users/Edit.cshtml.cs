using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public EditModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string TargetUserId { get; set; }

        public class InputModel
        {
            [Required]
            public string Id { get; set; }

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

            [Display(Name = "Yeni Şifre (İsteğe bağlı)")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required]
            [Display(Name = "Rol")]
            public string Role { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "User";

            Input = new InputModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = currentRole
            };
            
            TargetUserId = user.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Remove password validation if empty (it's optional)
                 if(string.IsNullOrEmpty(Input.NewPassword)) 
                 {
                    ModelState.Remove("Input.NewPassword");
                 }
                 
                 // If still invalid after check
                 if (!ModelState.IsValid && !string.IsNullOrEmpty(Input.NewPassword)) 
                     return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null) return NotFound();

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Email = Input.Email; // UserName update logic is complex, simpler to keep synced
            user.UserName = Input.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                 foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Update Role
            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();
            if (currentRole != Input.Role)
            {
                if (currentRole != null) await _userManager.RemoveFromRoleAsync(user, currentRole);
                await _userManager.AddToRoleAsync(user, Input.Role);
            }

            // Update Password if provided
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
                 if (!result.Succeeded)
                {
                     foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
