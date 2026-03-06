using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ProfileModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public PasswordModel PasswordInput { get; set; }

        public class InputModel
        {
            [Display(Name = "Kullanıcı Adı")]
            public string Username { get; set; }

            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Phone]
            [Display(Name = "Telefon Numarası")]
            public string PhoneNumber { get; set; }
        }

        public class PasswordModel
        {
            [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mevcut Şifre")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "Yeni şifre gereklidir.")]
            [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Yeni Şifre")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Yeni Şifre (Tekrar)")]
            [Compare("NewPassword", ErrorMessage = "Yeni şifre ve onay şifresi eşleşmiyor.")]
            public string ConfirmPassword { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                Username = userName,
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kullanıcı yüklenemedi ID: '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kullanıcı yüklenemedi ID: '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                if(ModelState.ContainsKey("PasswordInput.OldPassword")) ModelState.Remove("PasswordInput.OldPassword");
                if(ModelState.ContainsKey("PasswordInput.NewPassword")) ModelState.Remove("PasswordInput.NewPassword");
                if(ModelState.ContainsKey("PasswordInput.ConfirmPassword")) ModelState.Remove("PasswordInput.ConfirmPassword");
                
                // Re-check valid state after removing irrelevant password errors
                if(!ModelState.IsValid) {
                     await LoadAsync(user);
                     return Page();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Hata: Telefon numarası güncellenemedi.";
                    return RedirectToPage();
                }
            }

            if (Input.FirstName != user.FirstName || Input.LastName != user.LastName)
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                var updateResult = await _userManager.UpdateAsync(user);
                 if (!updateResult.Succeeded)
                {
                    StatusMessage = "Hata: Profil bilgileri güncellenemedi.";
                    return RedirectToPage();
                }
            }

            // Username Change
            if(Input.Username != user.UserName)
            {
                 var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Hata: Kullanıcı adı güncellenemedi (Belki bu isim başkası tarafından kullanılıyor).";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profiliniz güncellendi";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
             var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kullanıcı yüklenemedi ID: '{_userManager.GetUserId(User)}'.");
            }
            
            // Only validate PasswordInput properties
            ModelState.Clear();
            TryValidateModel(PasswordInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, PasswordInput.OldPassword, PasswordInput.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Şifreniz başarıyla değiştirildi.";

            return RedirectToPage();
        }
    }
}
