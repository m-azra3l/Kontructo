// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Kontructo.Models;
using Kontructo.Logic;

namespace Kontructo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        //private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger
            //IEmailSender emailSender
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            //_emailSender = emailSender;
            _roleManager = roleManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required, StringLength(30)]
            public string Name { get; set; }

            [Required, StringLength(30)]
            public string UserName { get; set; }

            [Required, StringLength(100)]
            public string Address { get; set; }

            [Required, StringLength(15)]
            public string DOB { get; set; }

            public byte[] Avatar { get; set; }

            [Required, StringLength(10)]
            public string Gender { get; set; }

            //delete the bool data type
            public bool IsMaster { get; set; }

            public bool IsSuperAdmin { get; set; }

            public bool IsAdmin { get; set; }

            public bool IsMember { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                byte[] imageData = null;
                foreach(var file in Request.Form.Files)
                {
                    MemoryStream ms = new MemoryStream();
                    file.CopyTo(ms);
                    if(ms.Length < 512000)
                    {
                        imageData = ms.ToArray();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The picture file is too large");
                        return Page();
                    }
                }
                var userNameExists = await _userManager.FindByNameAsync(Input.UserName);
                if(userNameExists != null)
                {
                    ModelState.AddModelError("", "Username already taken. Select a different username");
                    return RedirectToPage();

                }

                var emailExists = await _userManager.FindByEmailAsync(Input.Email);
                if (emailExists != null)
                {
                    ModelState.AddModelError("", "You already registered with this email.");
                    return RedirectToPage();

                }

                //var user = CreateUser()
                var user = new AppUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    Name = Input.Name,
                    Address = Input.Address,
                    DOB = Input.DOB,
                    Gender = Input.Gender
                };
                user.EmailConfirmed = true;
                user.Avatar = imageData;

                //await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //Create roles if not exisits
                    if (!await _roleManager.RoleExistsAsync(Roles.Master))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Roles.Master));
                    }
                    if (!await _roleManager.RoleExistsAsync(Roles.SuperAdmin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin));
                    }
                    if (!await _roleManager.RoleExistsAsync(Roles.Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(Roles.Member))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Roles.Member));
                    }

                    //Assign user to a role as per the check box selection

                    //await _userManager.AddToRoleAsync(user, Roles.Member);

                    //delete these ones
                    if (Input.IsMaster)
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Master);
                    }
                    else if (Input.IsSuperAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, Roles.SuperAdmin);
                    }
                    else if (Input.IsAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Admin);
                    }
                    else if (Input.IsMember)
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Member);
                    }

                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                     //   values: new { area = "Identity", userId = userId, returnUrl = returnUrl },
                     //   protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                       // $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("Login");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    _logger.LogInformation("Error.");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

      /*  private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }*/

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
