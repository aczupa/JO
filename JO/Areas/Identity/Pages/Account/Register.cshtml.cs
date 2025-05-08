using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JO.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RegisterModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Un compte avec cet e-mail existe déjà. Veuillez vous connecter.");
                    return Page();
                }

                
                if (!PasswordIsValid(Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Le mot de passe doit contenir au moins 10 caractères, dont une majuscule et un chiffre.");
                    return Page();
                }

                var identity = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(identity, Input.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(identity, isPersistent: false);
                    return LocalRedirect("~/");
                }
            }

            return Page();
        }

        
        private bool PasswordIsValid(string password)
        {
           
            return password.Length >= 10 && password.Any(char.IsUpper) && password.Any(char.IsDigit);
        }


    }
}

